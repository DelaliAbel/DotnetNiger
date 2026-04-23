// Composant securite Identity: RefreshTokenGenerator
using System.Security.Cryptography;
using System.Text;
using DotnetNiger.Identity.Application.Services.Interfaces;

namespace DotnetNiger.Identity.Infrastructure.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    // Generation d'un refresh token aleatoire.
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    // Hash SHA256 du refresh token pour stockage securise.
    string IRefreshTokenGenerator.HashToken(string token) => HashToken(token);

    // Static helper kept for repository compatibility.
    public static string HashToken(string token)
    {
        return HashTokenStatic(token);
    }

    private static string HashTokenStatic(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
