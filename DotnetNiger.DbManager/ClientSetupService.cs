using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using DotnetNiger.Common.Extensions;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.DbManager;

/// <summary>Configure les clients OIDC (web-ui, test-identity, test-client) et leurs permissions.</summary>
static class ClientSetupService
{
    /// <summary>Crée les clients OIDC de base et s'assure que web-ui a les bonnes permissions.</summary>
    public static async Task SetupAsync(IServiceProvider services)
    {
        var appManager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var db = services.GetRequiredService<IdentityDbContext>();

        var tenantId = await db.Tenants.Select(t => t.Id).FirstAsync();

        await CreatePublicClientAsync(appManager, "web-ui", "Web UI — Portail développeur",
            "http://localhost:5100/signin-oidc", "http://localhost:5100/",
            "http://localhost:5100/signout-callback-oidc",
            ["gt:authorization_code", "gt:external_login", "gt:refresh_token"]);

        await CreatePublicClientAsync(appManager, "test-identity", "TestIdentity — Application de test OIDC",
            "http://localhost:5200/signin-oidc", "http://localhost:5200/",
            "http://localhost:5200/signout-callback-oidc",
            ["gt:authorization_code", "gt:refresh_token"]);

        await CreateTestClientAsync(db, appManager, tenantId);
        await EnsureWebUiPermissionsAsync(appManager);
    }

    static async Task CreatePublicClientAsync(IOpenIddictApplicationManager appManager,
        string clientId, string displayName, string redirectUri,
        string postLogoutRedirectUri, string postLogoutCallbackUri, string[] extraGrants)
    {
        if (await appManager.FindByClientIdAsync(clientId) != null) return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId, ClientSecret = null, DisplayName = displayName,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        };
        descriptor.RedirectUris.Add(new Uri(redirectUri));
        descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutCallbackUri));
        descriptor.Permissions.Add("ept:token");
        descriptor.Permissions.Add("ept:authorization");
        descriptor.Permissions.Add("ept:logout");
        descriptor.Permissions.Add("ept:userinfo");
        foreach (var grant in extraGrants) descriptor.Permissions.Add(grant);
        descriptor.Permissions.Add("rst:code");
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add("scp:email");
        descriptor.Permissions.Add("scp:profile");
        descriptor.Permissions.Add("scp:roles");
        descriptor.Permissions.Add("scp:offline_access");
        await appManager.CreateAsync(descriptor);
    }

    static async Task CreateTestClientAsync(IdentityDbContext db, IOpenIddictApplicationManager appManager, Guid tenantId)
    {
        if (await appManager.FindByClientIdAsync("test-client") != null) return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "test-client", ClientSecret = "test-secret",
            DisplayName = "Test Client — Tests OAuth2",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
        };
        descriptor.Permissions.Add("ept:token");
        descriptor.Permissions.Add("ept:authorization");
        descriptor.Permissions.Add("ept:logout");
        descriptor.Permissions.Add("ept:userinfo");
        descriptor.Permissions.Add("gt:password");
        descriptor.Permissions.Add("gt:refresh_token");
        descriptor.Permissions.Add("gt:client_credentials");
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add("scp:email");
        descriptor.Permissions.Add("scp:profile");
        descriptor.Permissions.Add("scp:roles");
        descriptor.Permissions.Add("scp:offline_access");
        descriptor.Permissions.Add("scp:api");
        var app = await appManager.CreateAsync(descriptor);
        var appId = await appManager.GetIdAsync(app);

        db.TenantClients.Add(new TenantClient
        {
            Id = Guid.NewGuid(), TenantId = tenantId, ApplicationId = appId!,
            ClientId = "test-client", ClientSecretHash = "test-secret".HashSHA256(),
            ClientName = "Test Client", Description = "Client de test pour les tests OAuth2",
            RedirectUris = "[]", PostLogoutRedirectUris = "[]",
            AllowedGrantTypes = JsonSerializer.Serialize(new[] { "password", "refresh_token", "client_credentials" }),
            IsActive = true,
        });
        await db.SaveChangesAsync();
    }

    static async Task EnsureWebUiPermissionsAsync(IOpenIddictApplicationManager appManager)
    {
        try
        {
            var existing = await appManager.FindByClientIdAsync("web-ui");
            if (existing == null) return;

            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing);
            if (!descriptor.Permissions.Contains("gt:external_login"))
            {
                descriptor.Permissions.Add("gt:external_login");
                await appManager.UpdateAsync(existing, descriptor);
                Console.WriteLine("   Identity: added gt:external_login to web-ui client.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Warning: failed to ensure web-ui permissions: {ex.Message}");
        }
    }
}
