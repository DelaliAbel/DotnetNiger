using DotnetNiger.Gateway.Api.Filters;
using DotnetNiger.Gateway.Infrastructure.HttpClients;
using DotnetNiger.Gateway.Application.Services;
using DotnetNiger.Gateway.Application.Services.Interfaces;
using Microsoft.OpenApi.Models;

namespace DotnetNiger.Gateway.Api.Extensions;

/// <summary>
/// Enregistre tous les services du Gateway
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Enregistre les services du Gateway
    /// </summary>
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===== Services HTTP et Reverse Proxy =====
        services.AddHttpClient();

        // Clients API typés avec configuration
        services.AddHttpClient<IIdentityApiClient, IdentityApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(
                    configuration.GetValue<int>("Services:Identity:Timeout", 30));
            });

        services.AddHttpClient<ICommunityApiClient, CommunityApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(
                    configuration.GetValue<int>("Services:Community:Timeout", 30));
            });

        // ===== Services Métier =====
        services.AddScoped<IRequestForwardingService, RequestForwardingService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICachingService, CachingService>();
        services.AddScoped<IRateLimitService, RateLimitService>();
        services.AddScoped<IMetricsService, MetricsService>();

        // ===== Contrôleurs et Validation =====
        services.AddControllers();

        // ===== Présentation et Documentation =====
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DotnetNiger API Gateway",
                Version = "v1",
                Description = "Gateway d'agrégation pour tous les services microservices DotnetNiger",
                Contact = new OpenApiContact
                {
                    Name = "DotnetNiger Community",
                    Url = new Uri("https://github.com/DotnetNiger")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://github.com/DotnetNiger/LICENSE")
                }
            });

            // Options enrichies pour Swagger
        });

        // ===== Mise en Cache =====
        services.AddMemoryCache();

        // ===== CORS =====
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // ===== Reverse Proxy YARP =====
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        // ===== Filtres =====
        services.AddScoped<ExceptionFilter>();
        services.AddScoped<ValidationFilter>();

        return services;
    }
}

