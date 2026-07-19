using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Auth;

public partial class AuthService
{
    public string? GetRoleFromAccessToken(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        var segments = accessToken.Split('.');
        if (segments.Length < 2)
            return null;

        try
        {
            var payloadJson = Encoding.UTF8.GetString(JwtParser.ParseBase64WithoutPadding(segments[1]));
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;

            if (TryGetRoleValue(root, "roles", out var roleFromRoles))
                return roleFromRoles;

            if (TryGetRoleValue(root, "role", out var roleFromRole))
                return roleFromRole;

            if (TryGetRoleValue(root, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleFromClaimType))
                return roleFromClaimType;

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<(AuthDto?, string?)> ParseTokenResponseAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString()!;
            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

            var claims = ParseClaimsFromJwt(accessToken).ToList();

            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value ?? "";
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value ?? "";
            var fullName = claims.FirstOrDefault(c => c.Type is "name" or "full_name")?.Value ?? "";
            var avatarUrl = claims.FirstOrDefault(c => c.Type is "avatar_url" or "avatarUrl" or "picture")?.Value ?? "";
            var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var user = new UserDto
            {
                Id = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
                Email = email,
                FullName = fullName ?? email,
                Username = fullName ?? email,
                AvatarUrl = avatarUrl ?? string.Empty,
                IsActive = true,
                Roles = roles
            };

            var token = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = expiresIn
            };

            return (new AuthDto { User = user, Token = token }, null);
        }
        catch (Exception ex)
        {
            return (null, $"Erreur de lecture de la réponse: {ex.Message}");
        }
    }

    private static string? TryReadOidcError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("error_description", out var desc) && desc.ValueKind == JsonValueKind.String)
                return desc.GetString();
            if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                return err.GetString();
        }
        catch { }
        return null;
    }

    private static bool TryGetRoleValue(JsonElement root, string key, out string? role)
    {
        role = null;

        if (!root.TryGetProperty(key, out var roleElement))
            return false;

        if (roleElement.ValueKind == JsonValueKind.Array)
        {
            role = roleElement
                .EnumerateArray()
                .Select(x => x.GetString())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            return !string.IsNullOrWhiteSpace(role);
        }

        if (roleElement.ValueKind == JsonValueKind.String)
        {
            role = roleElement.GetString();
            return !string.IsNullOrWhiteSpace(role);
        }

        return false;
    }

    private static async Task<string?> TryReadErrorMessageAsync(HttpContent content)
    {
        try
        {
            var json = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString();

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                return message.GetString();

            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                return error.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        => JwtParser.ParseClaimsFromJwt(jwt);
}
