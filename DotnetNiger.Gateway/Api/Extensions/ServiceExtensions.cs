using DotnetNiger.Gateway.Api.Filters;

namespace DotnetNiger.Gateway.Api.Extensions;

/// <summary>
/// Enregistre les services
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        // Ajouter les filtres
        services.AddScoped<ExceptionFilter>();
        services.AddScoped<ValidationFilter>();

        return services;
    }
}
