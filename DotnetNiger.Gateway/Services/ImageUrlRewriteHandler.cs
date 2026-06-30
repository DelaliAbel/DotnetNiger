using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace DotnetNiger.Gateway.Services;

public class ImageUrlRewriteHandler : DelegatingHandler
{
    private readonly string _gatewayBaseUrl;

    public ImageUrlRewriteHandler(IConfiguration configuration)
    {
        _gatewayBaseUrl = (configuration["Gateway:BaseUrl"] ?? "https://dotnetniger.runasp.net").TrimEnd('/');
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode || response.Content == null)
            return response;

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType == null || !contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            return response;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!body.Contains("/uploads/", StringComparison.Ordinal))
            return response;

        var rewritten = body.Replace("\"/uploads/", $"\"{_gatewayBaseUrl}/uploads/", StringComparison.Ordinal);

        if (rewritten == body)
            return response;

        var newContent = new ByteArrayContent(Encoding.UTF8.GetBytes(rewritten));
        newContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        newContent.Headers.ContentLength = Encoding.UTF8.GetByteCount(rewritten);
        response.Content = newContent;
        return response;
    }
}
