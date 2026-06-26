using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.HealthChecks;
using DotnetNiger.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Ocelot.Provider.Consul;

namespace DotnetNiger.Gateway.Extensions;

/// <summary>Extensions pour l'enregistrement des services du Gateway dans le conteneur DI.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Enregistre tous les services nécessaires au Gateway : Ocelot, health checks, authentification, CORS, etc.</summary>
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

        services.AddHttpClient("Community", client =>
        {
            var devUrl = configuration.GetSection("DownstreamServices:Community:DevUrl").Value ?? "http://localhost:5050";
            client.BaseAddress = new Uri(devUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddSingleton<IOpenGraphService, OpenGraphService>();
        services.AddSingleton<OpenGraphHtmlBuilder>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerForOcelot(configuration);

        services.AddHealthChecks()
            .AddCheck<SmtpHealthCheck>("smtp", HealthStatus.Degraded, ["infrastructure", "email"])
            .AddCheck<DownstreamHealthCheck>("downstream", HealthStatus.Degraded, ["gateway", "connectivity"]);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var identityUrl = (configuration["DeveloperPortal:IdentityBaseUrl"] ?? "http://localhost:5075").TrimEnd('/');
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = !environment.IsDevelopment() && !configuration.GetValue<bool>("Jwt:DisableHttpsRequirement");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityUrl + "/",
                    ValidateAudience = true,
                    ValidAudience = "DotnetNiger.Identity.Client",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(context.Exception, "JWT authentication failed");
                        return Task.CompletedTask;
                    }
                };
            });

        services.Configure<List<DownstreamServiceConfig>>(configuration.GetSection("DownstreamServices"));

        var identityUrl = (configuration["DeveloperPortal:IdentityBaseUrl"] ?? "http://localhost:5075").TrimEnd('/');
        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(sp =>
        {
            var retriever = new HttpDocumentRetriever();
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            retriever.RequireHttps = !env.IsDevelopment() && !configuration.GetValue<bool>("Jwt:DisableHttpsRequirement");
            return new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{identityUrl}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                retriever);
        });

        return services;
    }


}
