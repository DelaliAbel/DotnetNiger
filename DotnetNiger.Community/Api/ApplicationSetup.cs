using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetNiger.Community.Api;
using Microsoft.OpenApi.Models;

namespace DotnetNiger.Community.Api;

/// <summary>Configure le builder de l'application Community.</summary>
public static class ApplicationSetup
{
    /// <summary>Crée et configure le WebApplicationBuilder.</summary>
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        builder.Services.AddProblemDetails();
        builder.Services.AddApiVersioningWithSwagger();
        builder.Services.AddCommunityInfrastructure(builder.Configuration);
        builder.Services.AddCommunityAuthentication(builder.Configuration, builder.Environment);
        builder.Services.AddCommunityServices();
        builder.Services.AddCommunityHttpClients(builder.Configuration);
        builder.Services.AddCommunityCors(builder.Configuration);

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DotnetNiger Community API",
                Version = "v1",
                Description = "API communautaire DotnetNiger"
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

        return builder;
    }
}
