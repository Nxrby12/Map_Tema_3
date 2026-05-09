namespace RestaurantApp.Application.Models;

public sealed class MenuSearchRequest
{
    public required string Keyword { get; init; }
    public required MenuSearchType SearchType { get; init; }
    public bool ShouldContain { get; init; }
}
