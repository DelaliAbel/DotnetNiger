using DotnetNiger.Identity.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

namespace DotnetNiger.Identity.Api.Extensions;

public static class ExternalAuthExtensions
{
    /// <summary>Configure le SmartScheme (JWT ou API Key) + authentification Google/GitHub si configurés.</summary>
    public static IServiceCollection AddExternalAuth(
        this IServiceCollection services, IConfiguration config)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "SmartScheme";
            options.DefaultChallengeScheme = "SmartScheme";
        })
        .AddPolicyScheme("SmartScheme", "JWT or API Key", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.ContainsKey("X-API-Key")
                    || context.Request.Headers.ContainsKey("X-Internal-Key"))
                    return ApiKeyAuthenticationDefaults.AuthenticationScheme;
                return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            };
        })
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationDefaults.AuthenticationScheme, null);

        AddGoogleIfConfigured(authBuilder, config);
        AddGitHubIfConfigured(authBuilder, config);

        return services;
    }

    private static void AddGoogleIfConfigured(AuthenticationBuilder authBuilder, IConfiguration config)
    {
        var googleId = config["Authentication:Google:ClientId"];
        if (string.IsNullOrEmpty(googleId)) return;

        authBuilder.AddGoogle(google =>
        {
            google.ClientId = googleId;
            google.ClientSecret = config["Authentication:Google:ClientSecret"] ?? "";
            google.SignInScheme = IdentityConstants.ExternalScheme;
            google.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            google.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            google.Events.OnRemoteFailure = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ctx.Failure, "Google OAuth remote failure: {Message}", ctx.Failure?.Message);
                ctx.Response.Redirect($"/Account/Login?error={Uri.EscapeDataString(ctx.Failure?.Message ?? "google_failed")}");
                ctx.HandleResponse();
                return Task.CompletedTask;
            };
        });
    }

    private static void AddGitHubIfConfigured(AuthenticationBuilder authBuilder, IConfiguration config)
    {
        var ghId = config["Authentication:GitHub:ClientId"];
        if (string.IsNullOrEmpty(ghId)) return;

        authBuilder.AddOAuth("GitHub", "GitHub", github =>
        {
            github.ClientId = ghId;
            github.ClientSecret = config["Authentication:GitHub:ClientSecret"] ?? "";
            github.SignInScheme = IdentityConstants.ExternalScheme;
            github.CallbackPath = "/signin-github";
            github.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            github.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            github.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            github.TokenEndpoint = "https://github.com/login/oauth/access_token";
            github.UserInformationEndpoint = "https://api.github.com/user";
            github.Scope.Add("user:email");

            github.Events.OnRemoteFailure = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ctx.Failure, "GitHub OAuth remote failure: {Message}", ctx.Failure?.Message);
                ctx.Response.Redirect($"/Account/Login?error={Uri.EscapeDataString(ctx.Failure?.Message ?? "github_failed")}");
                ctx.HandleResponse();
                return Task.CompletedTask;
            };
            github.Events.OnCreatingTicket = GitHubOnCreatingTicket;
        });
    }

    private static async Task GitHubOnCreatingTicket(OAuthCreatingTicketContext ctx)
    {
        if (ctx.Identity == null || ctx.AccessToken == null) return;

        var userReq = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        userReq.Headers.Authorization = new("Bearer", ctx.AccessToken);
        userReq.Headers.UserAgent.Add(new("DotnetNiger", "1.0"));
        userReq.Headers.Accept.Add(new("application/vnd.github.v3+json"));
        using var userResp = await ctx.Backchannel.SendAsync(userReq);
        userResp.EnsureSuccessStatusCode();
        var userEl = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
            await userResp.Content.ReadAsStringAsync());

        var userId = userEl.TryGetProperty("id", out var idEl) ? idEl.ToString() : null;
        var login = userEl.TryGetProperty("login", out var lEl) ? lEl.ToString() : null;
        var name = userEl.TryGetProperty("name", out var nEl) ? nEl.ToString() : null;
        var email = userEl.TryGetProperty("email", out var eEl) ? eEl.ToString() : null;

        if (userId != null)
        {
            ctx.Identity.AddClaim(new("sub", userId));
            ctx.Identity.AddClaim(new(System.Security.Claims.ClaimTypes.NameIdentifier, userId));
        }
        if (login != null)
            ctx.Identity.AddClaim(new(System.Security.Claims.ClaimTypes.Name, name ?? login));
        if (email != null)
            ctx.Identity.AddClaim(new(System.Security.Claims.ClaimTypes.Email, email));
        else
            await FetchGitHubPrimaryEmail(ctx);
    }

    private static async Task FetchGitHubPrimaryEmail(OAuthCreatingTicketContext ctx)
    {
        var emailReq = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        emailReq.Headers.Authorization = new("Bearer", ctx.AccessToken);
        emailReq.Headers.UserAgent.Add(new("DotnetNiger", "1.0"));
        using var emailResp = await ctx.Backchannel.SendAsync(emailReq);
        var emails = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(
            await emailResp.Content.ReadAsStringAsync());
        foreach (var item in emails ?? [])
            if (item.TryGetProperty("primary", out var p) && p.GetBoolean()
                && item.TryGetProperty("email", out var ev))
            {
                if (ctx.Identity is not null)
                    ctx.Identity.AddClaim(new(System.Security.Claims.ClaimTypes.Email, ev.GetString()!));
                break;
            }
    }
}
