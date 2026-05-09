using System.Security.Cryptography;
using System.Text;

namespace RestaurantApp.Application.Security;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
