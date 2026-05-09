namespace RestaurantApp.Domain.Entities;

public sealed class UserAccount
{
    public int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string Address { get; init; }
    public required string PasswordHash { get; init; }
    public required UserRole Role { get; init; }
}
