using System.Security.Claims;
using System.Threading.RateLimiting;
using DotnetNiger.Api.Middleware;
using DotnetNiger.Api.Middleware.ExceptionHandlers;
using DotnetNiger.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api.Extensions;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Enregistre les gestionnaires d'exceptions composables, dans l'ordre de priorité :
    /// chaque handler retourne false s'il ne gère pas l'exception et laisse la main au suivant.
    /// </summary>
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<UnauthorizedExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<InvalidOperationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddCorsFromConfig(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Value;
            var origins = !string.IsNullOrWhiteSpace(allowedOrigins)
                ? allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : [];

            options.AddDefaultPolicy(policy =>
            {
                if (origins.Length != 0)
                    policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                else if (isDevelopment)
                    policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader();
                else
                    policy.SetIsOriginAllowed(_ => false);
            });
        });
        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        var rateLimitOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? (int)retryAfterValue.TotalSeconds
                    : rateLimitOptions.WindowSeconds;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Trop de requêtes. Veuillez réessayer plus tard.",
                    retryAfterSeconds = retryAfter
                }, cancellationToken));
            };

            options.AddPolicy("default", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetTokenBucketLimiter(
                    $"default:{ip}",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateLimitOptions.PermitLimit,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                        TokensPerPeriod = rateLimitOptions.PermitLimit,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("Auth", httpContext =>
            {
                // Partition par utilisateur connecté (un abus ne pénalise pas tout le monde),
                // sinon par IP + ClientId.
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partitionKey = !string.IsNullOrEmpty(userId)
                    ? $"auth:user:{userId}"
                    : $"auth:{ipOrDefault(httpContext)}:{clientIdOrDefault(httpContext)}";
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateLimitOptions.AuthPermitLimit,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.AuthWindowSeconds),
                        TokensPerPeriod = rateLimitOptions.AuthPermitLimit,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Limiteur global (fallback pour tous les endpoints sans [EnableRateLimiting]) :
            // protège l'ensemble de l'API contre l'abus sans casser la navigation.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    $"global:{httpContext.Connection.RemoteIpAddress}",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateLimitOptions.GlobalPermitLimit,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                        TokensPerPeriod = rateLimitOptions.GlobalPermitLimit,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });
        return services;
    }

    private static string ipOrDefault(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string clientIdOrDefault(HttpContext httpContext) =>
        httpContext.Request.Headers["ClientId"].FirstOrDefault() ?? "unknown";

    public static WebApplication UsePipeline(this WebApplication app, bool isDevelopment)
    {
        if (isDevelopment)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // L'exception handler doit être le premier : il ne peut pas attraper ce qui a déjà tourné.
        app.UseExceptionHandler();
        app.UseEmptyErrorResponses();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.XFrameOptions = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });
        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

}
