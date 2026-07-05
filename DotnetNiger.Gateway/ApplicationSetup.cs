using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;

namespace DotnetNiger.Gateway;

/// <summary>Configure les services et le builder de l'application Gateway.</summary>
public static class ApplicationSetup
{
    /// <summary>Crée le WebApplicationBuilder avec Ocelot et CORS.</summary>
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var ocelotConfig = OcelotConfigMerger.Merge(builder.Configuration);
        builder.Configuration.AddConfiguration(ocelotConfig);
        builder.Services.AddOcelot().AddPolly();

        var jwtKey = builder.Configuration["Jwt:Key"];
        if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Bearer", o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
        }

        var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            if (!string.IsNullOrWhiteSpace(allowedOrigins))
                p.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                 .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            else
                p.AllowAnyOrigin();
        }));

        return builder;
    }
}
