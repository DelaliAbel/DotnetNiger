using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetNiger.Domain.Extensions;

public static partial class StringExtensions
{
    public static string Truncate(this string value, int maxLength, bool withEllipsis = false)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return withEllipsis ? value[..(maxLength - 3)] + "..." : value[..maxLength];
    }

    public static string RemoveAccents(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var bytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(value);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

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

    public static string NormalizeWhitespace(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return MultipleWhitespaceRegex().Replace(value.Trim(), " ");
    }

    public static string HashSHA256(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(bytes);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();
}
