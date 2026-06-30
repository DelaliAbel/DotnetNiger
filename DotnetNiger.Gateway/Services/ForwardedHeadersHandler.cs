using System.Net;
using System.Net.Http;

namespace DotnetNiger.Gateway.Services;

public class ForwardedHeadersHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForwardedHeadersHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", ctx.Request.Scheme);
            request.Headers.TryAddWithoutValidation("X-Forwarded-Host", ctx.Request.Host.Host);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
