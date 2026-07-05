using DotnetNiger.Identity.Api.Extensions;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;

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

        builder.Services.AddOpenApi();
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
