using RestaurantApp.Domain.Entities;

namespace RestaurantApp.Application.Contracts;

public interface IRestaurantRepository
{
    IReadOnlyList<Category> GetCategories();
    IReadOnlyList<MenuProduct> GetProducts();
    IReadOnlyList<UserAccount> GetUsers();
    IReadOnlyList<Order> GetOrders();
    void SaveOrder(Order order);
}
