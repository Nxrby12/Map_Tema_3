namespace RestaurantApp.Application.Models;

public sealed class RestaurantConfig
{
    public decimal MenuDiscountPercent { get; init; }
    public decimal BigOrderThreshold { get; init; }
    public int LoyalClientOrderCount { get; init; }
    public int LoyalClientDaysInterval { get; init; }
    public decimal OrderDiscountPercent { get; init; }
    public decimal FreeShippingThreshold { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal LowStockThreshold { get; init; }
}
