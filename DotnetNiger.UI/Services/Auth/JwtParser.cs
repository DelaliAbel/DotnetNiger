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
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) // On a besoin d'au moins Header et Payload
                return Enumerable.Empty<Claim>();

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            
            using var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;
            var claims = new List<Claim>();

            foreach (var prop in root.EnumerateObject())
            {
                var key = prop.Name;
                var value = prop.Value;

                if (key is "roles" or "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                {
                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in value.EnumerateArray())
                            claims.Add(new Claim(ClaimTypes.Role, r.GetString() ?? ""));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, value.GetString() ?? ""));
                    }
                    continue;
                }

                var claimType = JwtToClaimTypeMap.GetValueOrDefault(key, key);
                
                // Extraire la valeur proprement selon son type
                var valStr = value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => null,
                    _ => value.GetRawText()
                };

                if (valStr != null)
                    claims.Add(new Claim(claimType, valStr));
            }

            return claims;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du parsing JWT: {ex.Message}");
            return Enumerable.Empty<Claim>();
        }
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
