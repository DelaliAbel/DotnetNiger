using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Services;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Ocelot.Provider.Consul;

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

        services.Configure<List<DownstreamServiceConfig>>(configuration.GetSection("DownstreamServices"));

        return services;
    }


}
