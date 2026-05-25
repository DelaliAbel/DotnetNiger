using System.Text;
using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Ocelot.Provider.Consul;
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
            .AddPolly()
            .AddConsul();

        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddHostedService<ExternalServiceHealthService>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerForOcelot(configuration);

        services.AddCors(options =>
        {
            if (environment.IsDevelopment())
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            else
            {
                var origins = configuration["Cors:AllowedOrigins"];
                if (!string.IsNullOrWhiteSpace(origins))
                    options.AddPolicy("AllowAll", policy =>
                        policy.WithOrigins(origins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                              .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
                else
                    options.AddPolicy("AllowAll", policy =>
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
        });

        services.AddGatewayJwtAuthentication(configuration);

        services.Configure<List<DownstreamServiceConfig>>(configuration.GetSection("DownstreamServices"));

        return services;
    }

    private static void AddGatewayJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity";
        var audience = configuration["Jwt:Audience"] ?? "DotnetNiger.Identity.Client";

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
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtKey) && !jwtKey.StartsWith("__"),
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtKey) && !jwtKey.StartsWith("__"),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(jwtKey) && !jwtKey.StartsWith("__"),
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = !string.IsNullOrWhiteSpace(jwtKey) && jwtKey.Length >= 32 && !jwtKey.StartsWith("__")
                    ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    : null,
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

        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || jwtKey.StartsWith("__"))
            Log.Warning("JWT Key non configurée — routes avec AuthentificationProviderKey='Bearer' seront bypassées");
        else
            Log.Information("JWT Authentication configurée (Issuer={Issuer})", issuer);
    }
}
