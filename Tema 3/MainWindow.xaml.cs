using System.Windows;
using Tema_3.ViewModels;
using Tema_3;

namespace Tema_3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Această linie leagă design-ul de logica din ViewModel
            this.DataContext = new MenuViewModel();
        }

        // Metoda pentru butonul de Login
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWin = new LoginWindow();
            // Setăm ViewModel-ul ferestrei și îi dăm referința ferestrei pentru a o putea închide
            loginWin.DataContext = new LoginViewModel(loginWin);

            if (loginWin.ShowDialog() == true)
            {
                // Dacă login-ul a reușit, schimbăm starea în ViewModel-ul principal
                var mainVM = this.DataContext as MenuViewModel;
                mainVM.IsUserLoggedIn = true;
                // Aici poți verifica și dacă e Angajat pentru a-i arăta butoanele de Editare
            }
        }

        // Metoda pentru butonul de Cont Nou
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aici va apărea fereastra de Înregistrare.");
        }
    }
}