namespace RestaurantApp.Domain.Entities;

public enum OrderStatus
{
    Registered = 0,
    Preparing = 1,
    OnTheWay = 2,
    Delivered = 3,
    Cancelled = 4,
}
