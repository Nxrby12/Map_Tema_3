using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RestaurantApp.Application.Models;
using RestaurantApp.Application.Services;
using RestaurantApp.Domain.Entities;
using RestaurantApp.Infrastructure.Repositories;
using RestaurantApp.Presentation.Commands;

namespace RestaurantApp.Presentation.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RestaurantService _service;
    private UserAccount? _currentUser;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _searchKeyword = string.Empty;
    private MenuSearchType _selectedSearchType = MenuSearchType.Name;
    private bool _shouldContain = true;
    private MenuRow? _selectedMenuProduct;
    private int _orderQuantity = 1;
    private OrderRow? _selectedClientOrder;
    private OrderRow? _selectedEmployeeOrder;
    private OrderStatus _selectedOrderStatus = OrderStatus.Preparing;
    private string _statusMessage = "Vizualizare ca utilizator neautentificat";

    public MainViewModel()
    {
        var config = RestaurantConfigLoader.LoadFromAppSettings();

        _service = new RestaurantService(new InMemoryRestaurantRepository(config), config);

        LoginCommand = new RelayCommand(_ => Login());
        LogoutCommand = new RelayCommand(_ => Logout(), _ => _currentUser is not null);
        SearchCommand = new RelayCommand(_ => Search());
        ResetSearchCommand = new RelayCommand(_ => LoadMenu());
        PlaceOrderCommand = new RelayCommand(_ => PlaceOrder(), _ => IsClient && _selectedMenuProduct is not null && _orderQuantity > 0);
        RefreshClientOrdersCommand = new RelayCommand(_ => LoadClientOrders(), _ => IsClient);
        CancelOrderCommand = new RelayCommand(_ => CancelSelectedOrder(), _ => IsClient && _selectedClientOrder is not null);
        RefreshEmployeeOrdersCommand = new RelayCommand(_ => LoadEmployeeOrders(false), _ => IsEmployee);
        RefreshActiveEmployeeOrdersCommand = new RelayCommand(_ => LoadEmployeeOrders(true), _ => IsEmployee);
        UpdateOrderStatusCommand = new RelayCommand(_ => UpdateOrderStatus(), _ => IsEmployee && _selectedEmployeeOrder is not null);
        RefreshLowStockCommand = new RelayCommand(_ => LoadLowStock(), _ => IsEmployee);

        LoadMenu();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MenuRow> MenuItems { get; } = [];
    public ObservableCollection<OrderRow> ClientOrders { get; } = [];
    public ObservableCollection<OrderRow> EmployeeOrders { get; } = [];
    public ObservableCollection<LowStockRow> LowStockItems { get; } = [];

    public IEnumerable<MenuSearchType> SearchTypes => Enum.GetValues<MenuSearchType>();
    public IEnumerable<OrderStatus> UpdatableStatuses => Enum.GetValues<OrderStatus>().Where(s => s is not OrderStatus.Delivered and not OrderStatus.Cancelled);

    public string Email
    {
        get => _email;
        set => SetField(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => SetField(ref _searchKeyword, value);
    }

    public MenuSearchType SelectedSearchType
    {
        get => _selectedSearchType;
        set => SetField(ref _selectedSearchType, value);
    }

    public bool ShouldContain
    {
        get => _shouldContain;
        set => SetField(ref _shouldContain, value);
    }

    public MenuRow? SelectedMenuProduct
    {
        get => _selectedMenuProduct;
        set
        {
            if (SetField(ref _selectedMenuProduct, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public int OrderQuantity
    {
        get => _orderQuantity;
        set
        {
            if (SetField(ref _orderQuantity, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public OrderRow? SelectedClientOrder
    {
        get => _selectedClientOrder;
        set
        {
            if (SetField(ref _selectedClientOrder, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public OrderRow? SelectedEmployeeOrder
    {
        get => _selectedEmployeeOrder;
        set
        {
            if (SetField(ref _selectedEmployeeOrder, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public OrderStatus SelectedOrderStatus
    {
        get => _selectedOrderStatus;
        set => SetField(ref _selectedOrderStatus, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public bool IsClient => _currentUser?.Role == UserRole.Client;
    public bool IsEmployee => _currentUser?.Role == UserRole.Employee;

    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ResetSearchCommand { get; }
    public ICommand PlaceOrderCommand { get; }
    public ICommand RefreshClientOrdersCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand RefreshEmployeeOrdersCommand { get; }
    public ICommand RefreshActiveEmployeeOrdersCommand { get; }
    public ICommand UpdateOrderStatusCommand { get; }
    public ICommand RefreshLowStockCommand { get; }

    private void Login()
    {
        _currentUser = _service.Authenticate(Email.Trim(), Password);
        if (_currentUser is null)
        {
            StatusMessage = "Autentificare esuata";
            return;
        }

        StatusMessage = $"Autentificat: {_currentUser.FirstName} {_currentUser.LastName} ({_currentUser.Role})";
        LoadClientOrders();
        LoadEmployeeOrders(false);
        LoadLowStock();
        NotifyRolesChanged();
    }

    private void Logout()
    {
        _currentUser = null;
        Email = string.Empty;
        Password = string.Empty;
        StatusMessage = "Vizualizare ca utilizator neautentificat";
        ClientOrders.Clear();
        EmployeeOrders.Clear();
        LowStockItems.Clear();
        NotifyRolesChanged();
    }

    private void Search()
    {
        var results = _service.SearchMenu(new MenuSearchRequest
        {
            Keyword = SearchKeyword,
            SearchType = SelectedSearchType,
            ShouldContain = ShouldContain,
        });

        ReplaceMenu(results);
    }

    private void LoadMenu()
    {
        ReplaceMenu(_service.GetRestaurantMenu());
    }

    private void ReplaceMenu(IReadOnlyList<MenuProduct> products)
    {
        MenuItems.Clear();
        foreach (var product in products)
        {
            MenuItems.Add(new MenuRow(product));
        }
    }

    private void PlaceOrder()
    {
        if (SelectedMenuProduct is null)
        {
            return;
        }

        try
        {
            var order = _service.PlaceOrder(_currentUser, [(SelectedMenuProduct.Id, OrderQuantity)]);
            StatusMessage = $"Comanda {order.Code} a fost inregistrata";
            LoadClientOrders();
            LoadEmployeeOrders(false);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void LoadClientOrders()
    {
        ClientOrders.Clear();
        foreach (var order in _service.GetClientOrders(_currentUser))
        {
            ClientOrders.Add(new OrderRow(order));
        }
    }

    private void CancelSelectedOrder()
    {
        if (SelectedClientOrder is null)
        {
            return;
        }

        if (_service.CancelClientOrder(_currentUser, SelectedClientOrder.Code))
        {
            StatusMessage = $"Comanda {SelectedClientOrder.Code} a fost anulata";
            LoadClientOrders();
            LoadEmployeeOrders(false);
            return;
        }

        StatusMessage = "Comanda nu poate fi anulata";
    }

    private void LoadEmployeeOrders(bool activeOnly)
    {
        EmployeeOrders.Clear();
        foreach (var order in _service.GetOrdersForEmployee(_currentUser, activeOnly))
        {
            EmployeeOrders.Add(new OrderRow(order));
        }
    }

    private void UpdateOrderStatus()
    {
        if (SelectedEmployeeOrder is null)
        {
            return;
        }

        if (_service.UpdateOrderStatus(_currentUser, SelectedEmployeeOrder.Code, SelectedOrderStatus))
        {
            StatusMessage = $"Stare actualizata pentru {SelectedEmployeeOrder.Code}";
            LoadEmployeeOrders(false);
            LoadLowStock();
            LoadMenu();
            return;
        }

        StatusMessage = "Starea comenzii nu poate fi schimbata";
    }

    private void LoadLowStock()
    {
        LowStockItems.Clear();
        foreach (var row in _service.GetLowStockDishes(_currentUser))
        {
            LowStockItems.Add(new LowStockRow(row.Name, row.TotalQuantity));
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void NotifyRolesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsClient)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmployee)));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PlaceOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshClientOrdersCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CancelOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshEmployeeOrdersCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshActiveEmployeeOrdersCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (UpdateOrderStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshLowStockCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public sealed class MenuRow
    {
        public MenuRow(MenuProduct product)
        {
            Id = product.Id;
            Category = product.Category.Name;
            Name = product.Name;
            ProductType = product.ProductType.ToString();
            Portion = product.PortionDisplay;
            Price = product.Price;
            Allergens = product.Allergens.Count == 0 ? "-" : string.Join(", ", product.Allergens.Select(a => a.Name));
            Images = product.ImageUrls.Count == 0 ? "-" : string.Join(", ", product.ImageUrls);
            Availability = product.IsAvailable ? "Disponibil" : "Indisponibil";
            Components = product.Components.Count == 0
                ? "-"
                : string.Join("; ", product.Components.Select(c => $"{c.Dish.Name} ({c.PortionDisplay})"));
        }

        public int Id { get; }
        public string Category { get; }
        public string Name { get; }
        public string ProductType { get; }
        public string Portion { get; }
        public decimal Price { get; }
        public string Allergens { get; }
        public string Images { get; }
        public string Availability { get; }
        public string Components { get; }
    }

    public sealed class OrderRow
    {
        public OrderRow(Order order)
        {
            Code = order.Code;
            CreatedAt = order.CreatedAt;
            Lines = string.Join(", ", order.Lines.Select(l => $"{l.Quantity}x {l.Product.Name}"));
            FoodCost = order.FoodCost;
            ShippingCost = order.ShippingCost;
            Discount = order.DiscountValue;
            Total = order.TotalCost;
            Status = order.Status.ToString();
            Eta = order.EstimatedDeliveryTime;
            Client = $"{order.Client.LastName} {order.Client.FirstName} | {order.Client.Phone} | {order.Client.Address}";
        }

        public string Code { get; }
        public DateTime CreatedAt { get; }
        public string Lines { get; }
        public decimal FoodCost { get; }
        public decimal ShippingCost { get; }
        public decimal Discount { get; }
        public decimal Total { get; }
        public string Status { get; }
        public DateTime Eta { get; }
        public string Client { get; }
    }

    public sealed class LowStockRow(string dishName, decimal totalQuantity)
    {
        public string DishName { get; } = dishName;
        public decimal TotalQuantity { get; } = totalQuantity;
    }
}
