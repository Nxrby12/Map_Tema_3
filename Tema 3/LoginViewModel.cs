using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Tema_3.Models;

namespace Tema_3.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _context = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _email = string.Empty;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }

        public LoginViewModel(Window currentWindow)
        {
            LoginCommand = new RelayCommand<object>((p) =>
            {
                var passwordBox = p as System.Windows.Controls.PasswordBox;
                string password = passwordBox?.Password ?? "";

                var user = _context.Users
                    .FromSqlRaw("EXEC sp_LoginUser @Email, @Password",
                        new SqlParameter("@Email", Email),
                        new SqlParameter("@Password", password))
                    .AsEnumerable().FirstOrDefault();

                if (user != null)
                {
                    currentWindow.DialogResult = true;
                    currentWindow.Close();
                }
                else
                {
                    ErrorMessage = "Date incorecte!";
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}