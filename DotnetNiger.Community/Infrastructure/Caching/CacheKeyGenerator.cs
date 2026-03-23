namespace DotnetNiger.Community.Infrastructure.Caching;

public static class CacheKeyGenerator
{
    public static string Build(string scope, params object[] segments)
    {
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Cache scope is required", nameof(scope));

        var normalized = segments
            .Where(static s => s is not null)
            .Select(static s => s!.ToString()!.Trim())
            .Where(static s => !string.IsNullOrWhiteSpace(s));

        return $"community:{scope.ToLowerInvariant()}:{string.Join(':', normalized)}";
    }

    public static string Post(Guid postId) => Build("post", postId);
    public static string Resource(Guid resourceId) => Build("resource", resourceId);
    public static string Event(Guid eventId) => Build("event", eventId);
    public static string Search(string query, int page, int pageSize) => Build("search", query, page, pageSize);
}
