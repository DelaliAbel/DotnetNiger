using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Forward les requêtes aux microservices.
/// </summary>
public sealed class RequestForwardingService : IRequestForwardingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RequestForwardingService> _logger;

    public RequestForwardingService(
        IHttpClientFactory httpClientFactory,
        ILogger<RequestForwardingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> ForwardRequestAsync(string serviceUrl, HttpRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var targetUrl = $"{serviceUrl}{request.Path}{request.QueryString}";

        _logger.LogInformation("Forwarding request to: {Url}", targetUrl);

        var requestMessage = new HttpRequestMessage(
            new HttpMethod(request.Method),
            targetUrl);

        // Copier les headers
        foreach (var header in request.Headers)
        {
            if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        return await client.SendAsync(requestMessage);
    }
}
