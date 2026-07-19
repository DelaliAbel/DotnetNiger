using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class InternalApiKeyAuthAttribute : Attribute, IAuthorizationFilter
{
    private const string InternalKeyHeader = "X-Internal-Key";

    public InternalApiKeyAuthAttribute() { }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var configuredKey = configuration["InternalApiKey"] ?? "";

        if (string.IsNullOrEmpty(configuredKey))
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(InternalKeyHeader, out var values))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (values.FirstOrDefault() != configuredKey)
            context.Result = new UnauthorizedResult();
    }
}
