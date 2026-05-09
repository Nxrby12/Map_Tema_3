using System.IO;
using System.Text.Json;
using RestaurantApp.Application.Models;

namespace RestaurantApp.Presentation.ViewModels;

internal static class RestaurantConfigLoader
{
    private static readonly RestaurantConfig DefaultConfig = new()
    {
        MenuDiscountPercent = 10m,
        BigOrderThreshold = 120m,
        LoyalClientOrderCount = 3,
        LoyalClientDaysInterval = 30,
        OrderDiscountPercent = 15m,
        FreeShippingThreshold = 100m,
        ShippingCost = 15m,
        LowStockThreshold = 5m,
    };

    public static RestaurantConfig LoadFromAppSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return DefaultConfig;
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<RestaurantConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            return config ?? DefaultConfig;
        }
        catch
        {
            return DefaultConfig;
        }
    }
}
