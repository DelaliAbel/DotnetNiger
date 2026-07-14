using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace DotnetNiger.Gateway;

public static partial class SocialPreviewMiddleware
{
    private static readonly HashSet<string> CrawlerPatterns =
    [
        "facebookexternalhit", "facebot", "twitterbot", "linkedinbot",
        "whatsapp", "slackbot", "telegrambot", "discordbot",
        "pinterest", "googlebot", "bingbot", "slurp", "duckduckbot",
        "baiduspider", "yandexbot", "skypeuripreview",
        "microdatagenerator", "w3c_unicorn",
        "vkshare", "applebot", "embedly"
    ];

    private static readonly (string Pattern, string ApiPath, string Type)[] Routes =
    [
        ("^/blog/([^/]+)$",            "/api/v1/posts/by-slug/{0}",     "article"),
        ("^/evenements/([^/]+)$",      "/api/v1/events/by-slug/{0}",   "website"),
        ("^/ressource/([^/]+)$",       "/api/v1/resources/by-slug/{0}", "website"),
        ("^/ressources/([^/]+)$",      "/api/v1/resources/by-slug/{0}", "website")
    ];

    public static void UseSocialPreview(this WebApplication app)
    {
        var communityUrl = app.Configuration["DownstreamServices:Community:DevUrl"] ?? "http://localhost:5050";
        var baseUrl = app.Configuration["Gateway:BaseUrl"] ?? "http://localhost:5000";

        app.Use(async (ctx, next) =>
        {
            var userAgent = ctx.Request.Headers.UserAgent.ToString();

            if (string.IsNullOrWhiteSpace(userAgent) || !IsCrawler(userAgent))
            {
                await next();
                return;
            }

            var path = ctx.Request.Path.Value?.TrimEnd('/') ?? "";

            foreach (var (pattern, apiPath, ogType) in Routes)
            {
                var match = Regex.Match(path, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (!match.Success) continue;

                var slug = match.Groups[1].Value;
                var downstreamUrl = $"{communityUrl.TrimEnd('/')}{string.Format(apiPath, slug)}";

                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    var response = await client.GetAsync(downstreamUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var ogData = JsonSerializer.Deserialize<OgApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (ogData?.Data is not null)
                        {
                            var fullUrl = $"{baseUrl.TrimEnd('/')}{path}";
                            var imageUrl = ogData.Data.ImageUrl;

                            if (!string.IsNullOrWhiteSpace(imageUrl) && !imageUrl.StartsWith("http"))
                                imageUrl = $"{baseUrl.TrimEnd('/')}{imageUrl}";

                            var title = HttpUtility.HtmlEncode(ogData.Data.Title);
                            var description = HttpUtility.HtmlEncode(ogData.Data.Description);
                            var encodedImageUrl = HttpUtility.HtmlEncode(imageUrl);
                            var encodedFullUrl = HttpUtility.HtmlEncode(fullUrl);

                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/html; charset=utf-8";
                            await ctx.Response.WriteAsync($"""
                            <!DOCTYPE html>
                            <html lang="fr">
                            <head>
                                <meta charset="utf-8">
                                <title>{title} - .NET Niger</title>
                                <meta property="og:title" content="{title}" />
                                <meta property="og:description" content="{description}" />
                                <meta property="og:image" content="{encodedImageUrl}" />
                                <meta property="og:url" content="{encodedFullUrl}" />
                                <meta property="og:type" content="{ogType}" />
                                <meta name="twitter:card" content="summary_large_image" />
                                <meta name="twitter:title" content="{title}" />
                                <meta name="twitter:description" content="{description}" />
                                <meta name="twitter:image" content="{encodedImageUrl}" />
                            </head>
                            <body>
                                <h1>{title}</h1>
                                <p>{description}</p>
                            </body>
                            </html>
                            """);
                            return;
                        }
                    }
                }
                catch
                {
                    // Fall through to next() on error
                }
                break;
            }

            await next();
        });
    }

    private static bool IsCrawler(string userAgent)
    {
        foreach (var pattern in CrawlerPatterns)
            if (userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private sealed class OgApiResponse
    {
        public bool Success { get; set; }
        public OgMetadata? Data { get; set; }
    }

    private sealed class OgMetadata
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}
