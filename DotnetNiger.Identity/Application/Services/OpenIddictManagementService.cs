using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using OpenIddict.Abstractions;

namespace DotnetNiger.Identity.Application.Services;

public class OpenIddictManagementService
{
    private readonly IdentityDbContext _db;

    public OpenIddictManagementService(IdentityDbContext db) => _db = db;

    public async Task<string> BootstrapWebUiAsync(IOpenIddictApplicationManager appManager, string frontendBaseUrl)
    {
        var existing = await appManager.FindByClientIdAsync("web-ui");
        if (existing != null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing);
            AddWebUiPermissions(descriptor);
            descriptor.RedirectUris.Add(new Uri($"{frontendBaseUrl.TrimEnd('/')}/signin-oidc"));
            descriptor.PostLogoutRedirectUris.Add(new Uri(frontendBaseUrl.TrimEnd('/') + "/"));
            await appManager.UpdateAsync(existing, descriptor);
            return "web-ui client updated";
        }

        var newDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "web-ui",
            ClientSecret = null,
            DisplayName = "Web UI — Portail développeur",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        };

        newDescriptor.RedirectUris.Add(new Uri($"{frontendBaseUrl.TrimEnd('/')}/signin-oidc"));
        newDescriptor.PostLogoutRedirectUris.Add(new Uri(frontendBaseUrl.TrimEnd('/') + "/"));
        AddWebUiPermissions(newDescriptor);
        await appManager.CreateAsync(newDescriptor);
        return "web-ui client created";
    }

    public async Task<(string clientId, string clientSecret)> CreateDefaultClientAsync(
        IOpenIddictApplicationManager appManager, Tenant tenant)
    {
        var clientId = $"app_{Guid.NewGuid():N}";
        var clientSecret = GenerateSecret();
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = $"{tenant.Name} — Application par défaut",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        };

        AddDefaultClientPermissions(descriptor);
        var app = await appManager.CreateAsync(descriptor)
            ?? throw new InvalidOperationException("Échec création application OpenIddict");

        var applicationId = await appManager.GetIdAsync(app);
        var tenantClient = new TenantClient
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            ApplicationId = applicationId!,
            ClientId = clientId,
            ClientSecretHash = HashSecret(clientSecret),
            ClientName = $"{tenant.Name} — Application par défaut",
            RedirectUris = "[]",
            PostLogoutRedirectUris = "[]",
            AllowedGrantTypes = JsonSerializer.Serialize(new[]
            {
                "authorization_code", "password", "refresh_token", "client_credentials"
            }),
            IsActive = true,
        };

        _db.TenantClients.Add(tenantClient);
        await _db.SaveChangesAsync();
        return (clientId, clientSecret);
    }

    private static void AddWebUiPermissions(OpenIddictApplicationDescriptor descriptor)
    {
        descriptor.Permissions.Add("ept:token");
        descriptor.Permissions.Add("ept:authorization");
        descriptor.Permissions.Add("ept:logout");
        descriptor.Permissions.Add("ept:userinfo");
        descriptor.Permissions.Add("gt:authorization_code");
        descriptor.Permissions.Add("gt:external_login");
        descriptor.Permissions.Add("gt:refresh_token");
        descriptor.Permissions.Add("rst:code");
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add("scp:email");
        descriptor.Permissions.Add("scp:profile");
        descriptor.Permissions.Add("scp:roles");
        descriptor.Permissions.Add("scp:offline_access");
    }

    private static void AddDefaultClientPermissions(OpenIddictApplicationDescriptor descriptor)
    {
        descriptor.Permissions.Add("ept:token");
        descriptor.Permissions.Add("ept:authorization");
        descriptor.Permissions.Add("ept:logout");
        descriptor.Permissions.Add("ept:userinfo");
        descriptor.Permissions.Add("gt:authorization_code");
        descriptor.Permissions.Add("gt:password");
        descriptor.Permissions.Add("gt:refresh_token");
        descriptor.Permissions.Add("gt:client_credentials");
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add("scp:email");
        descriptor.Permissions.Add("scp:profile");
        descriptor.Permissions.Add("scp:roles");
        descriptor.Permissions.Add("scp:offline_access");
        descriptor.Permissions.Add("scp:api");
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(bytes);
    }
}
