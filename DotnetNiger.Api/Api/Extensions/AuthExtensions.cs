using System.Security.Claims;
using System.Text;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Infrastructure.Auth;
using DotnetNiger.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotnetNiger.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });
        return services;
    }

    public static IServiceCollection AddOAuthProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var googleSection = configuration.GetSection("Authentication:Google");
        if (!string.IsNullOrEmpty(googleSection["ClientId"]) && googleSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
        {
            services.AddAuthentication().AddGoogle("Google", options =>
            {
                options.ClientId = googleSection["ClientId"]!;
                options.ClientSecret = googleSection["ClientSecret"]!;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });
        }

        var githubSection = configuration.GetSection("Authentication:GitHub");
        if (!string.IsNullOrEmpty(githubSection["ClientId"]) && githubSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
        {
            services.AddAuthentication().AddOAuth("GitHub", options =>
            {
                options.ClientId = githubSection["ClientId"]!;
                options.ClientSecret = githubSection["ClientSecret"]!;
                options.CallbackPath = "/signin-github";
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Scope.Add("user:email");

                options.Events.OnCreatingTicket = async context =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                    using var response = await context.Backchannel.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (user.RootElement.TryGetProperty("id", out var id))
                        context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.ToString() ?? ""));
                    if (user.RootElement.TryGetProperty("login", out var login))
                        context.Identity?.AddClaim(new Claim(ClaimTypes.Name, login.ToString() ?? ""));
                    if (user.RootElement.TryGetProperty("email", out var email) && !email.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
                        context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email.ToString() ?? ""));
                };
            });
        }

        var microsoftSection = configuration.GetSection("Authentication:Microsoft");
        if (!string.IsNullOrEmpty(microsoftSection["ClientId"]) && microsoftSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
        {
            services.AddAuthentication().AddMicrosoftAccount("Microsoft", options =>
            {
                options.ClientId = microsoftSection["ClientId"]!;
                options.ClientSecret = microsoftSection["ClientSecret"]!;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Scope.Add("https://graph.microsoft.com/User.Read");
            });
        }

        return services;
    }

    public static IServiceCollection ConfigureCookieAuthentication(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { error = "Non authentifié" });
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { error = "Accès refusé" });
            };
        });
        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
        });
        return services;
    }
}
