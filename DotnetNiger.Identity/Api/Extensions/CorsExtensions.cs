namespace DotnetNiger.Identity.Api.Extensions;

public static class CorsExtensions
{
    /// <summary>Configure CORS avec les origines autorisées depuis la config (fallback AllowAnyOrigin).</summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration config)
    {
        var allowedOrigins = config["Cors:AllowedOrigins"] ?? "";
        var origins = allowedOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendOrigins", policy =>
            {

                if (origins.Length > 0)
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                else
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        return services;
    }
}
