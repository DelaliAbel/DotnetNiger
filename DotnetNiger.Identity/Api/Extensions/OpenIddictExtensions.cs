using DotnetNiger.Identity.Infrastructure;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace DotnetNiger.Identity.Api.Extensions;

public static class OpenIddictExtensions
{
    /// <summary>Configure OpenIddict Core + Server + Validation avec certificat et endpoint passthrough.</summary>
    public static IServiceCollection AddOpenIddictServer(
        this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOpenIddict()
            .AddCore(core => core.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
            .AddServer(server =>
            {
                var issuerUri = config.GetValue<string>("OpenIddict:Issuer");
                if (!string.IsNullOrWhiteSpace(issuerUri))
                    server.SetIssuer(new Uri(issuerUri));

                server.SetTokenEndpointUris("/connect/token")
                      .SetAuthorizationEndpointUris("/connect/authorize")
                      .SetLogoutEndpointUris("/connect/logout")
                      .SetUserinfoEndpointUris("/connect/userinfo");

                server.AllowPasswordFlow()
                      .AllowRefreshTokenFlow()
                      .AllowAuthorizationCodeFlow()
                          .RequireProofKeyForCodeExchange()
                      .AllowClientCredentialsFlow()
                      .AllowCustomFlow("external_login")
                      .SetRefreshTokenLifetime(TimeSpan.FromDays(14))
                      .SetRefreshTokenReuseLeeway(TimeSpan.FromSeconds(0));

                server.DisableAccessTokenEncryption();
                LoadCertificate(server, config, env);

                var aspNetCore = server.UseAspNetCore()
                      .EnableTokenEndpointPassthrough()
                      .EnableAuthorizationEndpointPassthrough()
                      .EnableLogoutEndpointPassthrough();

                if (env.IsDevelopment() || config.GetValue<bool>("OpenIddict:DisableTransportSecurityRequirement"))
                    aspNetCore.DisableTransportSecurityRequirement();

                server.RegisterScopes(
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Email,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Roles,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.OfflineAccess,
                    "api");
            })
            .AddValidation(validation =>
            {
                validation.UseLocalServer();
                validation.UseAspNetCore();
            });

        return services;
    }

    private static void LoadCertificate(OpenIddictServerBuilder server, IConfiguration config, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            var certPath = Path.Combine(env.ContentRootPath, "..", "https", "localhost.pfx");
            certPath = Path.GetFullPath(certPath);
            var certPassword = "1234";

            if (File.Exists(certPath))
            {
                try
                {
                    var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(
                        File.ReadAllBytes(certPath), certPassword);
                    server.AddEncryptionCertificate(cert).AddSigningCertificate(cert);
                    return;
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to load PFX from {certPath}, using ephemeral keys", ex);
                }
            }
            else
            {
                LogWarning($"PFX not found at {certPath}, using ephemeral keys");
            }

            server.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
            server.IgnoreEndpointPermissions()
                  .IgnoreGrantTypePermissions()
                  .IgnoreScopePermissions();
            server.AcceptAnonymousClients();
        }
        else
        {
            var certPath = config["OpenIddict:CertificatePath"] ?? "/etc/ssl/certs/opendict.pfx";
            var certPassword = config["OpenIddict:CertificatePassword"] ?? "";
            if (File.Exists(certPath))
            {
                var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(
                    File.ReadAllBytes(certPath), certPassword);
                server.AddEncryptionCertificate(cert).AddSigningCertificate(cert);
            }
            else
            {
                server.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
            }
        }
    }

    private static void LogWarning(string message, Exception? ex = null)
    {
        var factory = LoggerFactory.Create(b => b.AddConsole());
        var logger = factory.CreateLogger("OpenIddict");
        if (ex != null) logger.LogWarning(ex, message);
        else logger.LogWarning(message);
    }
}
