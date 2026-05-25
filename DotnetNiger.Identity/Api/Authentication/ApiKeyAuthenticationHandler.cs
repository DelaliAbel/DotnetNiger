using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Api.Authentication;

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
        if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyValues))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.NoResult();

        var hashString = HashApiKey(apiKey);

        var tenantContext = Context.RequestServices.GetRequiredService<TenantContext>();
        var db = Context.RequestServices.GetRequiredService<IdentityDbContext>();

        IQueryable<TenantApiKey> query = db.TenantApiKeys
            .Where(k => k.PrivateKeyHash == hashString && k.IsActive);

        if (tenantContext.TenantId.HasValue)
            query = query.Where(k => k.TenantId == tenantContext.TenantId.Value);

        var storedKey = await query.FirstOrDefaultAsync();

        if (storedKey == null)
            return AuthenticateResult.Fail("Invalid or inactive API key");

        storedKey.LastUsedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, storedKey.TenantId.ToString()),
            new Claim("tenant_id", storedKey.TenantId.ToString()),
            new Claim("api_key_id", storedKey.Id.ToString()),
            new Claim("api_key_prefix", storedKey.KeyPrefix),
        };

        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string HashApiKey(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }
}
