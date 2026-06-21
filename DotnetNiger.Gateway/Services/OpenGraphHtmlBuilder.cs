using System.Web;

namespace DotnetNiger.Gateway.Services;

public class OpenGraphHtmlBuilder
{
    private readonly string _frontendBaseUrl;

    public OpenGraphHtmlBuilder(IConfiguration configuration)
    {
        _frontendBaseUrl = (configuration["FrontendBaseUrl"] ?? "http://localhost:5100").TrimEnd('/');
    }

    public string Build(OGMetadata meta, string slug, string type)
    {
        var frontendUrl = $"{_frontendBaseUrl}/{type}/{HttpUtility.UrlEncode(slug)}";
        var title = HttpUtility.HtmlEncode(meta.Title);
        var description = HttpUtility.HtmlEncode(meta.Description);
        var imageUrl = ResolveImageUrl(meta.ImageUrl);

        return $"""
        <!DOCTYPE html>
        <html lang="fr">
        <head>
          <meta charset="utf-8" />
          <title>{title} — Dotnet Niger</title>
          <meta property="og:title" content="{title}" />
          <meta property="og:description" content="{description}" />
          <meta property="og:image" content="{imageUrl}" />
          <meta property="og:url" content="{frontendUrl}" />
          <meta property="og:type" content="article" />
          <meta property="og:site_name" content="Dotnet Niger" />
          <meta property="og:locale" content="fr_FR" />
          <meta name="twitter:card" content="summary_large_image" />
          <meta name="twitter:title" content="{title}" />
          <meta name="twitter:description" content="{description}" />
          <meta name="twitter:image" content="{imageUrl}" />
          <meta http-equiv="refresh" content="0; url={frontendUrl}" />
        </head>
        <body>
          <script>window.location.href = "{frontendUrl}";</script>
        </body>
        </html>
        """;
    }

    public string BuildDefault()
    {
        return $"""
        <!DOCTYPE html>
        <html lang="fr">
        <head>
          <meta charset="utf-8" />
          <title>Dotnet Niger</title>
          <meta property="og:title" content="Dotnet Niger" />
          <meta property="og:description" content="Communauté .NET du Niger" />
          <meta property="og:image" content="{_frontendBaseUrl}/images/og-default.jpg" />
          <meta property="og:url" content="{_frontendBaseUrl}" />
          <meta property="og:type" content="website" />
          <meta property="og:site_name" content="Dotnet Niger" />
          <meta property="og:locale" content="fr_FR" />
          <meta name="twitter:card" content="summary_large_image" />
        </head>
        <body></body>
        </html>
        """;
    }

    private string ResolveImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return $"{_frontendBaseUrl}/images/og-default.jpg";

        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return imageUrl;

        return $"{_frontendBaseUrl}/{imageUrl.TrimStart('/')}";
    }
}
