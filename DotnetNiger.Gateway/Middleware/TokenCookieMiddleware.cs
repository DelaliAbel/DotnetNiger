using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DotnetNiger.Gateway.Middleware;

public class TokenCookieMiddleware
{
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";

    private readonly RequestDelegate _next;
    private readonly string _identityBaseUrl;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenCookieMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
        IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _identityBaseUrl = (configuration["DeveloperPortal:IdentityBaseUrl"] ?? "http://localhost:5075").TrimEnd('/');
        _configurationManager = configurationManager;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        // --- Local cookie auth endpoints (handled here, before Ocelot) ---
        if (path.Equals("/api/auth/tokens", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                await HandlePostTokens(context);
                return;
            }
            if (string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
            {
                HandleDeleteTokens(context);
                return;
            }
        }

        if (path.Equals("/api/auth/session", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            await HandleGetSession(context);
            return;
        }

        if (path.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            await HandleRefreshToken(context);
            return;
        }

        // --- Inject Bearer header from httpOnly cookie before Ocelot ---
        if (!context.Request.Headers.ContainsKey("Authorization") &&
            context.Request.Cookies.TryGetValue(AccessTokenCookie, out var token) &&
            !string.IsNullOrWhiteSpace(token))
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        await _next(context);
    }

    private async Task HandlePostTokens(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(body);
        }
        catch
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "Invalid JSON" }));
            return;
        }

        using (doc)
        {
            var root = doc.RootElement;
            var accessToken = root.TryGetProperty("accessToken", out var at) ? at.GetString() : null;
            var refreshToken = root.TryGetProperty("refreshToken", out var rt) ? rt.GetString() : null;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "accessToken is required" }));
                return;
            }

            SetTokenCookie(context, AccessTokenCookie, accessToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
                SetTokenCookie(context, RefreshTokenCookie, refreshToken);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { message = "Tokens stored in secure cookies" }));
        }
    }

    private void HandleDeleteTokens(HttpContext context)
    {
        ClearTokenCookie(context, AccessTokenCookie);
        ClearTokenCookie(context, RefreshTokenCookie);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        context.Response.WriteAsync(
            JsonSerializer.Serialize(new { message = "Tokens cleared" }));
    }

    private async Task HandleGetSession(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        if (!context.Request.Cookies.TryGetValue(AccessTokenCookie, out var token) ||
            string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { authenticated = false }));
            return;
        }

        try
        {
            var oidcConfig = await _configurationManager.GetConfigurationAsync(context.RequestAborted);

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _identityBaseUrl + "/",
                ValidateAudience = true,
                ValidAudience = "DotnetNiger.Identity.Client",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = oidcConfig.SigningKeys,
                ClockSkew = TimeSpan.FromMinutes(1),
            }, out _);

            var claims = principal.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { authenticated = true, claims }));
        }
        catch (SecurityTokenExpiredException)
        {
            ClearTokenCookie(context, AccessTokenCookie);
            ClearTokenCookie(context, RefreshTokenCookie);
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { authenticated = false, reason = "token_expired" }));
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<TokenCookieMiddleware>>();
            logger.LogWarning(ex, "Session validation failed");
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { authenticated = false, reason = "invalid_token", detail = ex.Message }));
        }
    }

    private async Task HandleRefreshToken(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        if (!context.Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "refresh_token manquant" }));
            return;
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = "openid profile email roles offline_access"
            };

            var response = await client.PostAsync(
                $"{_identityBaseUrl}/connect/token",
                new FormUrlEncodedContent(formData),
                context.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                ClearTokenCookie(context, AccessTokenCookie);
                ClearTokenCookie(context, RefreshTokenCookie);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "Refresh token invalide ou expiré" }));
                return;
            }

            var json = await response.Content.ReadAsStringAsync(context.RequestAborted);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var newAccessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var newRefreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

            if (string.IsNullOrWhiteSpace(newAccessToken))
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "Réponse invalide du serveur d'authentification" }));
                return;
            }

            SetTokenCookie(context, AccessTokenCookie, newAccessToken);
            if (!string.IsNullOrWhiteSpace(newRefreshToken))
                SetTokenCookie(context, RefreshTokenCookie, newRefreshToken);

            var claims = ParseClaimsFromJwt(newAccessToken).ToList();
            var userIdClaim = claims.FirstOrDefault(c => c.Type is "sub" or "nameidentifier");
            var emailClaim = claims.FirstOrDefault(c => c.Type is "email" or "emailaddress");
            var userInfo = new
            {
                userId = string.IsNullOrEmpty(userIdClaim.Type) ? null : userIdClaim.Value,
                email = string.IsNullOrEmpty(emailClaim.Type) ? null : emailClaim.Value,
                roles = claims.Where(c => c.Type == "role").Select(c => c.Value).ToList()
            };

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { success = true, user = userInfo }));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "Erreur lors du rafraîchissement du token", detail = ex.Message }));
        }
    }

    private static void SetTokenCookie(HttpContext context, string name, string value)
    {
        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(14),
        });
    }

    private static void ClearTokenCookie(HttpContext context, string name)
    {
        context.Response.Cookies.Append(name, "", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UnixEpoch,
        });
    }

    private static IEnumerable<(string Type, string Value)> ParseClaimsFromJwt(string jwt)
    {
        var segments = jwt.Split('.');
        if (segments.Length < 2)
            return [];

        try
        {
            var payload = segments[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            var padding = (payload.Length % 4) switch
            {
                2 => "==",
                3 => "=",
                _ => ""
            };
            var jsonBytes = Convert.FromBase64String(payload + padding);
            using var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;

            return root.EnumerateObject()
                .SelectMany(p =>
                {
                    if (p.Name is "roles" or "role")
                    {
                        if (p.Value.ValueKind == JsonValueKind.Array)
                            return p.Value.EnumerateArray()
                                .Select(v => ("role", v.GetString() ?? ""));
                        return [("role", p.Value.GetString() ?? "")];
                    }
                    return [(p.Name, p.Value.ToString())];
                })
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}

