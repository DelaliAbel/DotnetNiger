using System.Text;
using DotnetNiger.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Serilog;

namespace DotnetNiger.Gateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddOcelot(configuration)
            .AddCacheManager(x => x.WithDictionaryHandle())
            .AddPolly();

        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerForOcelot(configuration);

        services.AddCors(options =>
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddGatewayJwtAuthentication(configuration);

        services.Configure<List<DownstreamServiceConfig>>(configuration.GetSection("DownstreamServices"));

        return services;
    }

    private static void AddGatewayJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || jwtKey.StartsWith("__"))
        {
            Log.Warning("JWT Key non configurée ou invalide — authentification désactivée");
            return;
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity",
                ValidAudience = configuration["Jwt:Audience"] ?? "DotnetNiger.Identity.Client",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        context.Token = authHeader["Bearer ".Length..].Trim();
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                    var isPublicPath = path.StartsWith("/api/diagnostics", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);

                    if (!isPublicPath)
                        Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);

                    return Task.CompletedTask;
                }
            };
        });

        Log.Information("JWT Authentication configurée (Issuer={Issuer})",
            configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity");
    }
}
