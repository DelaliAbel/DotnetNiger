using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetNiger.Common.Extensions;

/// <summary>
/// Méthodes d'extension pour les chaînes de caractères.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Tronque une chaîne à une longueur maximale, avec ou sans points de suspension.
    /// </summary>
    public static string Truncate(this string value, int maxLength, bool withEllipsis = false)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return withEllipsis ? value[..(maxLength - 3)] + "..." : value[..maxLength];
    }

    /// <summary>
    /// Supprime les accents d'une chaîne.
    /// </summary>
    public static string RemoveAccents(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var bytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(value);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Vérifie si une chaîne est un email valide (format simple).
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Normalise une chaîne : supprime les espaces superflus.
    /// </summary>
    public static string NormalizeWhitespace(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return MultipleWhitespaceRegex().Replace(value.Trim(), " ");
    }

    /// <summary>
    /// Calcule le hash SHA256 d'une chaîne en Base64.
    /// </summary>
    public static string HashSHA256(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(bytes);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();
}
