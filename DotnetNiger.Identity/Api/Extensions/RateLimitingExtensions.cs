using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetNiger.Identity.Api.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>Configure le rate limiting : TenantRegistration, Auth (fenêtre fixe) + limite globale 500/IP/min.</summary>
    public static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services, IConfiguration config)
    {
        var permitLimit = int.TryParse(config["RateLimiting:PermitLimit"], out var p) ? p : 5;
        var windowSeconds = int.TryParse(config["RateLimiting:WindowSeconds"], out var w) ? w : 60;
        var authPermitLimit = int.TryParse(config["RateLimiting:AuthPermitLimit"], out var ap) ? ap : 20;
        var authWindowSeconds = int.TryParse(config["RateLimiting:AuthWindowSeconds"], out var aw) ? aw : 60;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("TenantRegistration", opt =>
            {
                opt.PermitLimit = permitLimit;
                opt.Window = TimeSpan.FromSeconds(windowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("Auth", opt =>
            {
                opt.PermitLimit = authPermitLimit;
                opt.Window = TimeSpan.FromSeconds(authWindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 500,
                        Window = TimeSpan.FromSeconds(60),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
