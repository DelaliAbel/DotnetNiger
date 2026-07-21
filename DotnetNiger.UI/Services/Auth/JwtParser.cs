using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DotnetNiger.UI.Services.Auth;

public static class JwtParser
{
    private static readonly Dictionary<string, string> JwtToClaimTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sub"] = ClaimTypes.NameIdentifier,
        ["email"] = ClaimTypes.Email,
        ["name"] = ClaimTypes.Name,
        ["given_name"] = ClaimTypes.GivenName,
        ["family_name"] = ClaimTypes.Surname,
        ["picture"] = "avatar_url",
    };

    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 3)
            return Enumerable.Empty<Claim>();

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kvs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

        return kvs.SelectMany(kv =>
        {
            if (kv.Key is "roles" or "role")
            {
                if (kv.Value.ValueKind == JsonValueKind.Array)
                    return kv.Value.EnumerateArray().Select(r => new Claim(ClaimTypes.Role, r.GetString()!));
                return new[] { new Claim(ClaimTypes.Role, kv.Value.GetString()!) };
            }

            var claimType = JwtToClaimTypeMap.GetValueOrDefault(kv.Key, kv.Key);
            return new[] { new Claim(claimType, kv.Value.ToString()) };
        });
    }

    public static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return (base64.Length % 4) switch
        {
            2 => Convert.FromBase64String(base64 + "=="),
            3 => Convert.FromBase64String(base64 + "="),
            _ => Convert.FromBase64String(base64),
        };
    }
}
