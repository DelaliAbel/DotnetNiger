using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Infrastructure.Data;
using DotnetNiger.Domain.Entities;

namespace DotnetNiger.Infrastructure.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyValues)
            && !Request.Headers.TryGetValue("X-Internal-Key", out apiKeyValues))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.NoResult();

        var config = Context.RequestServices.GetRequiredService<IConfiguration>();
        var internalApiKey = config["InternalApiKey"] ?? "";

        if (!string.IsNullOrEmpty(internalApiKey) && apiKey == internalApiKey)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "internal-api"),
                new(ClaimTypes.Role, RoleConstants.Admin),
                new(ClaimTypes.Role, RoleConstants.SuperAdmin),
                new("client_id", "internal-api"),
            };

            var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        var dbCtx = Context.RequestServices.GetRequiredService<DotnetNigerDbContext>();

        IQueryable<ApiKey> query = dbCtx.ApiKeys
            .Where(k => k.IsActive);

        var allActiveKeys = await query.ToListAsync();

        var storedKey = allActiveKeys.FirstOrDefault(k => VerifyApiKey(apiKey, k.PrivateKeyHash));

        if (storedKey == null)
            return AuthenticateResult.Fail("Invalid or inactive API key");

        if (storedKey.LastUsedAt == null || DateTime.UtcNow - storedKey.LastUsedAt.Value > TimeSpan.FromMinutes(5))
        {
            storedKey.LastUsedAt = DateTime.UtcNow;
            await dbCtx.SaveChangesAsync();
        }

        var keyClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, storedKey.Id.ToString()),
            new("api_key_id", storedKey.Id.ToString()),
            new("api_key_prefix", storedKey.KeyPrefix),
        };

        var scopeList = JsonSerializer.Deserialize<string[]>(storedKey.Scopes) ?? [];
        foreach (var scope in scopeList)
            keyClaims.Add(new Claim("scope", scope));

        var keyIdentity = new ClaimsIdentity(keyClaims, ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var keyPrincipal = new ClaimsPrincipal(keyIdentity);
        var keyTicket = new AuthenticationTicket(keyPrincipal, Scheme.Name);

        return AuthenticateResult.Success(keyTicket);
    }

    private static bool VerifyApiKey(string apiKey, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedKey = Convert.FromBase64String(parts[1]);

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(apiKey),
            salt,
            600_000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(storedKey, keyBytes);
    }

    public static string HashApiKey(string apiKey)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(apiKey),
            salt,
            600_000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(keyBytes)}";
    }
}
