using System.Windows;
using RestaurantApp.Presentation.ViewModels;

namespace RestaurantApp.Presentation;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
