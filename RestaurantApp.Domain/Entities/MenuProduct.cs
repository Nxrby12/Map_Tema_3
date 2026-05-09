namespace RestaurantApp.Domain.Entities;

public sealed class MenuProduct
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required Category Category { get; init; }
    public required MenuProductType ProductType { get; init; }
    public decimal Price { get; set; }
    public required string PortionDisplay { get; init; }
    public decimal TotalQuantity { get; set; }
    public IReadOnlyCollection<Allergen> Allergens { get; init; } = [];
    public IReadOnlyCollection<string> ImageUrls { get; init; } = [];
    public IReadOnlyCollection<MenuComponent> Components { get; init; } = [];

    public bool IsAvailable => ProductType == MenuProductType.Dish
        ? TotalQuantity > 0
        : Components.All(c => c.Dish.TotalQuantity > 0);
}
