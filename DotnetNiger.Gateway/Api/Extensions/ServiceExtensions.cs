using DotnetNiger.Gateway.Api.Filters;
using DotnetNiger.Gateway.Infrastructure.HttpClients;
using DotnetNiger.Gateway.Application.Services;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Api.Extensions;

/// <summary>
/// Enregistre tous les services du Gateway
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===== Clients API typés =====
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

        // ===== Mise en Cache =====
        services.AddMemoryCache();

        // ===== Filtres =====
        services.AddScoped<ExceptionFilter>();
        services.AddScoped<ValidationFilter>();

        return services;
    }
}

