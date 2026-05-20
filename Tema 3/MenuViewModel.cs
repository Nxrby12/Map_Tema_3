using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Tema_3.Models;

namespace Tema_3.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _context;
        public event PropertyChangedEventHandler? PropertyChanged;

        // Proprietăți pentru Meniu și Căutare
        public ObservableCollection<Category> CategoriesWithProducts { get; set; } = new();

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(); LoadMenu(); }
        }

        // Proprietăți pentru Coș și Preț (Rezolvă erorile CS0103)
        public ObservableCollection<Product> CartItems { get; set; } = new();

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; OnPropertyChanged(); }
        }

        private bool _isUserLoggedIn = false;
        public bool IsUserLoggedIn
        {
            get => _isUserLoggedIn;
            set { _isUserLoggedIn = value; OnPropertyChanged(); }
        }

        // Comenzi
        public ICommand SearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public MenuViewModel()
        {
            _context = new RestaurantDbContext();

            // Inițializăm comenzile
            SearchCommand = new RelayCommand(LoadMenu);
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            PlaceOrderCommand = new RelayCommand(PlaceOrder, () => CartItems.Any() && IsUserLoggedIn);

            LoadMenu();
        }

        private void LoadMenu()
        {
            CategoriesWithProducts.Clear();
            var query = _context.Categories.Include(c => c.Products).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                // Exemplu simplu de filtrare
                var categories = query.ToList();
                foreach (var cat in categories)
                {
                    var filtered = cat.Products.Where(p => p.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (filtered.Any())
                    {
                        cat.Products = filtered;
                        CategoriesWithProducts.Add(cat);
                    }
                }
            }
            else
            {
                foreach (var cat in query.ToList())
                {
                    CategoriesWithProducts.Add(cat);
                }
            }
        }

        private void AddToCart(Product? product)
        {
            if (product != null)
            {
                CartItems.Add(product);
                CalculateFinalPrice();
            }
        }

        // Rezolvă eroarea CS0103 pentru CalculateFinalPrice
        private void CalculateFinalPrice()
        {
            decimal subtotal = CartItems.Sum(p => p.Price);

            // Valorile implicite dacă nu sunt în App.config
            decimal a = 100; // Prag transport
            decimal b = 15;  // Taxă transport
            decimal y = 200; // Prag discount
            decimal w = 5;   // Procent discount

            decimal total = subtotal;

            if (total > y) total -= total * (w / 100);
            if (total < a && total > 0) total += b;

            TotalPrice = total;
        }

        private void PlaceOrder()
        {
            try
            {
                int currentUserId = 1; // Ar trebui preluat de la userul logat
                var orderIdParam = new SqlParameter("@OrderID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _context.Database.ExecuteSqlRaw("EXEC sp_CreateOrder @ClientID, @TotalCost, @Status, @OrderID OUTPUT",
                    new SqlParameter("@ClientID", currentUserId),
                    new SqlParameter("@TotalCost", TotalPrice),
                    new SqlParameter("@Status", "Inregistrata"),
                    orderIdParam);

                int newOrderId = (int)orderIdParam.Value;

                foreach (var item in CartItems)
                {
                    _context.Database.ExecuteSqlRaw("EXEC sp_AddOrderItemAndReduceStock @OrderID, @ProductID, @Quantity",
                        new SqlParameter("@OrderID", newOrderId),
                        new SqlParameter("@ProductID", item.ProductId),
                        new SqlParameter("@Quantity", 1));
                }

                MessageBox.Show($"Comandă plasată cu succes!");
                CartItems.Clear();
                CalculateFinalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}