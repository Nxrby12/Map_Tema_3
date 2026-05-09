using RestaurantApp.Application.Contracts;
using RestaurantApp.Application.Models;
using RestaurantApp.Domain.Entities;

namespace RestaurantApp.Infrastructure.Repositories;

public sealed class InMemoryRestaurantRepository : IRestaurantRepository
{
    private readonly List<Category> _categories;
    private readonly List<Allergen> _allergens;
    private readonly List<MenuProduct> _products;
    private readonly List<UserAccount> _users;
    private readonly List<Order> _orders = [];

    public InMemoryRestaurantRepository(RestaurantConfig config)
    {
        _categories =
        [
            new Category { Id = 1, Name = "Mic dejun" },
            new Category { Id = 2, Name = "Supe/Ciorbe" },
            new Category { Id = 3, Name = "Fel principal" },
            new Category { Id = 4, Name = "Desert" },
            new Category { Id = 5, Name = "Bauturi" },
        ];

        _allergens =
        [
            new Allergen { Id = 1, Name = "gluten" },
            new Allergen { Id = 2, Name = "lactoza" },
            new Allergen { Id = 3, Name = "oua" },
            new Allergen { Id = 4, Name = "telina" },
            new Allergen { Id = 5, Name = "peste" },
        ];

        var soup = new MenuProduct
        {
            Id = 1,
            Name = "Supa crema de ciuperci",
            Category = _categories[1],
            ProductType = MenuProductType.Dish,
            PortionDisplay = "300g",
            Price = 22m,
            TotalQuantity = 12m,
            Allergens = [_allergens[1], _allergens[3]],
            ImageUrls = ["https://example.local/soup.jpg"],
        };

        var fish = new MenuProduct
        {
            Id = 2,
            Name = "Pastrav pane",
            Category = _categories[2],
            ProductType = MenuProductType.Dish,
            PortionDisplay = "200g",
            Price = 35m,
            TotalQuantity = 8m,
            Allergens = [_allergens[0], _allergens[4]],
            ImageUrls = ["https://example.local/fish.jpg"],
        };

        var fries = new MenuProduct
        {
            Id = 3,
            Name = "Cartofi prajiti",
            Category = _categories[2],
            ProductType = MenuProductType.Dish,
            PortionDisplay = "200g",
            Price = 12m,
            TotalQuantity = 30m,
            Allergens = [],
            ImageUrls = ["https://example.local/fries.jpg"],
        };

        var dessert = new MenuProduct
        {
            Id = 4,
            Name = "Cheesecake",
            Category = _categories[3],
            ProductType = MenuProductType.Dish,
            PortionDisplay = "150g",
            Price = 18m,
            TotalQuantity = 2m,
            Allergens = [_allergens[0], _allergens[1], _allergens[2]],
            ImageUrls = ["https://example.local/cheesecake.jpg"],
        };

        var fishAndChips = new MenuProduct
        {
            Id = 100,
            Name = "Fish & Chips",
            Category = _categories[2],
            ProductType = MenuProductType.Bundle,
            PortionDisplay = "200g/200g",
            Price = 0m,
            TotalQuantity = 0m,
            Allergens = [_allergens[0], _allergens[4]],
            ImageUrls = ["https://example.local/fishandchips.jpg"],
            Components =
            [
                new MenuComponent { Dish = fish, PortionDisplay = fish.PortionDisplay },
                new MenuComponent { Dish = fries, PortionDisplay = fries.PortionDisplay },
            ],
        };

        fishAndChips.Price = Math.Round(fishAndChips.Components.Sum(c => c.Dish.Price) * (1m - config.MenuDiscountPercent / 100m), 2);

        _products = [soup, fish, fries, dessert, fishAndChips];

        _users =
        [
            new UserAccount
            {
                Id = 1,
                FirstName = "Ana",
                LastName = "Client",
                Email = "client@example.com",
                Phone = "0711000000",
                Address = "Str. Florilor 10",
                Password = "client123",
                Role = UserRole.Client,
            },
            new UserAccount
            {
                Id = 2,
                FirstName = "Mihai",
                LastName = "Angajat",
                Email = "employee@example.com",
                Phone = "0722000000",
                Address = "Str. Lalelelor 20",
                Password = "employee123",
                Role = UserRole.Employee,
            },
        ];
    }

    public IReadOnlyList<Category> GetCategories() => _categories;

    public IReadOnlyList<MenuProduct> GetProducts() => _products;

    public IReadOnlyList<UserAccount> GetUsers() => _users;

    public IReadOnlyList<Order> GetOrders() => _orders;

    public void SaveOrder(Order order) => _orders.Add(order);
}
