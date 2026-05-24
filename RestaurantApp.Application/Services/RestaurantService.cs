using RestaurantApp.Application.Contracts;
using RestaurantApp.Application.Models;
using RestaurantApp.Application.Security;
using RestaurantApp.Domain.Entities;

namespace RestaurantApp.Application.Services;

public sealed class RestaurantService
{
    private readonly IRestaurantRepository _repository;
    private readonly RestaurantConfig _config;

    public RestaurantService(IRestaurantRepository repository, RestaurantConfig config)
    {
        _repository = repository;
        _config = config;
    }

    public IReadOnlyList<MenuProduct> GetRestaurantMenu() => _repository
        .GetProducts()
        .OrderBy(p => p.Category.Name)
        .ThenBy(p => p.Name)
        .ToList();

    public IReadOnlyList<MenuProduct> SearchMenu(MenuSearchRequest request)
    {
        var keyword = request.Keyword.Trim();
        if (keyword.Length == 0)
        {
            return GetRestaurantMenu();
        }

        var products = _repository.GetProducts().AsEnumerable();
        return request.SearchType switch
        {
            MenuSearchType.Name => products.Where(p => ApplyContainsRule(p.Name, keyword, request.ShouldContain)).ToList(),
            MenuSearchType.Allergen => products.Where(p => ApplyContainsRule(string.Join(',', p.Allergens.Select(a => a.Name)), keyword, request.ShouldContain)).ToList(),
            _ => [],
        };
    }

    public UserAccount? Authenticate(string email, string password)
    {
        return _repository.GetUsers().FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(u.PasswordHash, PasswordHasher.Hash(password), StringComparison.Ordinal));
    }

    public Order PlaceOrder(UserAccount? user, IReadOnlyList<(int productId, int quantity)> lines)
    {
        if (user is null || user.Role != UserRole.Client)
        {
            throw new InvalidOperationException("Doar clientii autentificati pot plasa comenzi.");
        }

        var productsById = _repository.GetProducts().ToDictionary(p => p.Id);
        var orderLines = new List<OrderLine>();

        foreach (var (productId, quantity) in lines.Where(l => l.quantity > 0))
        {
            if (!productsById.TryGetValue(productId, out var product) || !product.IsAvailable)
            {
                continue;
            }

            orderLines.Add(new OrderLine
            {
                Product = product,
                Quantity = quantity,
                UnitPrice = product.Price,
            });
        }

        if (orderLines.Count == 0)
        {
            throw new InvalidOperationException("Comanda trebuie sa contina cel putin un produs disponibil.");
        }

        var now = DateTime.UtcNow;
        var foodCost = orderLines.Sum(l => l.LineTotal);
        var discount = ComputeDiscount(user, now, foodCost);
        var shipping = foodCost < _config.FreeShippingThreshold ? _config.ShippingCost : 0m;

        var order = new Order
        {
            Id = _repository.GetOrders().Count + 1,
            Code = $"CMD-{now:yyyyMMdd}-{_repository.GetOrders().Count + 1:D4}",
            Client = user,
            CreatedAt = now,
            Lines = orderLines,
            FoodCost = foodCost,
            DiscountValue = discount,
            ShippingCost = shipping,
            TotalCost = foodCost - discount + shipping,
            EstimatedDeliveryTime = now.AddMinutes(45),
            Status = OrderStatus.Registered,
        };

        _repository.SaveOrder(order);
        return order;
    }

    public IReadOnlyList<Order> GetClientOrders(UserAccount? user)
    {
        if (user is null || user.Role != UserRole.Client)
        {
            return [];
        }

        return _repository.GetOrders()
            .Where(o => string.Equals(o.Client.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public IReadOnlyList<Order> GetOrdersForEmployee(UserAccount? user, bool activeOnly)
    {
        if (user is null || user.Role != UserRole.Employee)
        {
            return [];
        }

        var orders = _repository.GetOrders().AsEnumerable();
        if (activeOnly)
        {
            orders = orders.Where(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled);
        }

        return orders.OrderByDescending(o => o.CreatedAt).ToList();
    }

    public bool CancelClientOrder(UserAccount? user, string orderCode)
    {
        if (user is null || user.Role != UserRole.Client)
        {
            return false;
        }

        var order = _repository.GetOrders().FirstOrDefault(o =>
            string.Equals(o.Code, orderCode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(o.Client.Email, user.Email, StringComparison.OrdinalIgnoreCase));

        if (order is null || order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return false;
        }

        order.Status = OrderStatus.Cancelled;
        return true;
    }

    public bool UpdateOrderStatus(UserAccount? user, string orderCode, OrderStatus status)
    {
        if (user is null || user.Role != UserRole.Employee)
        {
            return false;
        }

        var order = _repository.GetOrders().FirstOrDefault(o => string.Equals(o.Code, orderCode, StringComparison.OrdinalIgnoreCase));
        if (order is null || order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return false;
        }

        order.Status = status;
        if (status == OrderStatus.Delivered)
        {
            ConsumeStock(order);
        }

        return true;
    }

    public IReadOnlyList<(string Name, decimal TotalQuantity)> GetLowStockDishes(UserAccount? user)
    {
        if (user is null || user.Role != UserRole.Employee)
        {
            return [];
        }

        return _repository.GetProducts()
            .Where(p => p.ProductType == MenuProductType.Dish && p.TotalQuantity <= _config.LowStockThreshold)
            .Select(p => (p.Name, p.TotalQuantity))
            .OrderBy(t => t.Name)
            .ToList();
    }

    private decimal ComputeDiscount(UserAccount user, DateTime now, decimal foodCost)
    {
        var hasValueDiscount = foodCost > _config.BigOrderThreshold;
        var loyalWindowStart = now.AddDays(-_config.LoyalClientDaysInterval);

        var loyalOrderCount = _repository.GetOrders().Count(o =>
            string.Equals(o.Client.Email, user.Email, StringComparison.OrdinalIgnoreCase) &&
            o.CreatedAt >= loyalWindowStart);

        var hasLoyalDiscount = loyalOrderCount >= _config.LoyalClientOrderCount;
        if (!hasValueDiscount && !hasLoyalDiscount)
        {
            return 0m;
        }

        return Math.Round(foodCost * _config.OrderDiscountPercent / 100m, 2);
    }

    private static bool ApplyContainsRule(string source, string keyword, bool shouldContain)
    {
        var hasKeyword = source.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        return shouldContain ? hasKeyword : !hasKeyword;
    }

    private static void ConsumeStock(Order order)
    {
        foreach (var line in order.Lines)
        {
            if (line.Product.ProductType == MenuProductType.Dish)
            {
                line.Product.TotalQuantity = Math.Max(0m, line.Product.TotalQuantity - line.Quantity);
                continue;
            }

            foreach (var component in line.Product.Components)
            {
                component.Dish.TotalQuantity = Math.Max(0m, component.Dish.TotalQuantity - line.Quantity);
            }
        }
    }
}
