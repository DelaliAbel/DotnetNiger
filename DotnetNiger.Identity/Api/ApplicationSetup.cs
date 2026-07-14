using System.Reflection;
using DotnetNiger.Identity.Api.Extensions;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

namespace DotnetNiger.Identity.Api;

/// <summary>Configure le builder de l'application Identity.</summary>
public static class ApplicationSetup
{
    /// <summary>Crée et configure le WebApplicationBuilder.</summary>
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApplicationSetup).Assembly);
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DotnetNiger Identity API",
                Version = "v1",
                Description = "API d'authentification et de gestion des utilisateurs DotnetNiger"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Entrez votre token JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{typeof(ApplicationSetup).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddIdentityCore(builder.Configuration);
        builder.Services.AddOpenIddictServer(builder.Configuration, builder.Environment);
        builder.Services.AddExternalAuth(builder.Configuration);
        builder.Services.AddCorsPolicy(builder.Configuration);
        builder.Services.AddIdentityServices();
        builder.Services.AddRateLimitingPolicies(builder.Configuration);
        builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformer>();
        builder.Services.AddApiVersioningWithSwagger();

        return builder;
    }
}
