using System.Text.RegularExpressions;
using DotnetNiger.Gateway.Services;

namespace DotnetNiger.Gateway.Middleware;

public class OpenGraphMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] CrawlerPatterns =
    [
        "facebookexternalhit",
        "Twitterbot",
        "LinkedInBot",
        "WhatsApp",
        "Slack",
        "Discordbot",
        "TelegramBot",
        "Pinterest"
    ];

    private static readonly Regex RouteRegex = new(
        @"^\/(blog|evenements|ressources)\/([^\/\?]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public OpenGraphMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOpenGraphService ogService, OpenGraphHtmlBuilder htmlBuilder)
    {
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent) || !IsCrawler(userAgent))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var match = RouteRegex.Match(path);

        if (!match.Success)
        {
            var defaultHtml = htmlBuilder.BuildDefault();
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(defaultHtml);
            return;
        }

        var type = match.Groups[1].Value;
        var slug = match.Groups[2].Value;

        var meta = await ogService.FetchMetadataAsync(type, slug);

        if (meta is null || string.IsNullOrWhiteSpace(meta.Title))
        {
            var defaultHtml = htmlBuilder.BuildDefault();
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(defaultHtml);
            return;
        }

        var html = htmlBuilder.Build(meta, slug, type);
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
    }

    private static bool IsCrawler(string userAgent)
    {
        return CrawlerPatterns.Any(p =>
            userAgent.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
