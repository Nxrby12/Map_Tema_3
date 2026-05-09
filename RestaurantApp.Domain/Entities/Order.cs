namespace RestaurantApp.Domain.Entities;

public sealed class Order
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required UserAccount Client { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<OrderLine> Lines { get; init; } = [];
    public decimal FoodCost { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime EstimatedDeliveryTime { get; set; }
    public OrderStatus Status { get; set; }
}
