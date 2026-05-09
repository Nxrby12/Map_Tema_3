namespace RestaurantApp.Domain.Entities;

public sealed class OrderLine
{
    public required MenuProduct Product { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    public decimal LineTotal => Quantity * UnitPrice;
}
