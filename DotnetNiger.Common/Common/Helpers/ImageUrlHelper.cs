namespace DotnetNiger.Common.Helpers;

/// <summary>Helper pour construire des URLs absolues d'images et de ressources.</summary>
public static class ImageUrlHelper
{
    /// <summary>Transforme un chemin relatif en URL absolue en le préfixant avec le <paramref name="baseUrl"/>.</summary>
    /// <param name="relativePath">Chemin relatif (ex: "/uploads/blog/guid.jpg").</param>
    /// <param name="baseUrl">URL de base (ex: "https://api-community.example.com").</param>
    public static string ToAbsolute(string? relativePath, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}
