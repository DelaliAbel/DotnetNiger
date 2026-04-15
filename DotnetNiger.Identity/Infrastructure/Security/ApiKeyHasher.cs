// Composant securite Identity: ApiKeyHasher
using System.Security.Cryptography;
using System.Text;

namespace DotnetNiger.Identity.Infrastructure.Security;

// Utilitaire de hash pour les cles API avec sel (HMAC-SHA256).
public static class ApiKeyHasher
{
    private const int SaltSize = 16;

    /// <summary>
    /// Hash une cle API avec un sel aleatoire. Retourne "sel_base64:hash_base64".
    /// </summary>
    public static string Hash(string key)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHmac(key, salt);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifie une cle API brute contre un hash stocke ("sel:hash").
    /// Supporte aussi l'ancien format SHA256 simple (migration transparente).
    /// </summary>
    public static bool Verify(string rawKey, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(rawKey) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        // Nouveau format: "sel:hash"
        var separatorIndex = storedHash.IndexOf(':');
        if (separatorIndex > 0)
        {
            var salt = Convert.FromBase64String(storedHash[..separatorIndex]);
            var expectedHash = Convert.FromBase64String(storedHash[(separatorIndex + 1)..]);
            var computedHash = ComputeHmac(rawKey, salt);
            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }

        // Ancien format: SHA256 simple (retro-compatibilite)
        var legacyHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        var legacyStored = Convert.FromBase64String(storedHash);
        return CryptographicOperations.FixedTimeEquals(legacyHash, legacyStored);
    }

    private static byte[] ComputeHmac(string key, byte[] salt)
    {
        using var hmac = new HMACSHA256(salt);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
    }
}
