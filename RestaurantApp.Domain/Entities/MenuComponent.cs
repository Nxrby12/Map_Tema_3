namespace RestaurantApp.Domain.Entities;

public sealed class MenuComponent
{
    public required MenuProduct Dish { get; init; }
    public required string PortionDisplay { get; init; }
}
