namespace RestaurantApp.Domain.Entities;

public sealed class Allergen
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
