namespace DotnetNiger.Domain.Helpers;

public static class ImageUrlHelper
{
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
