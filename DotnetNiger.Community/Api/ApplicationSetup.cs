using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetNiger.Community.Api;

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
        builder.Services.AddOpenApi();

        return builder;
    }
}
