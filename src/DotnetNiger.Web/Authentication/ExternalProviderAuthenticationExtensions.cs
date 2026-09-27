using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Enregistrement des fournisseurs d'identité externe pilotés par la configuration.
/// </summary>
public static class ExternalProviderAuthenticationExtensions
{
    /// <summary>
    /// Ajoute les fournisseurs pour lesquels un identifiant et un secret sont présents.
    /// </summary>
    /// <param name="builder">Constructeur d'authentification de l'application.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <remarks>
    /// Un fournisseur sans identifiant n'est pas enregistré du tout : l'application démarre
    /// sans lui plutôt que d'afficher un bouton qui échouerait. Le secret n'est jamais loggé.
    /// </remarks>
    public static AuthenticationBuilder AddConfiguredExternalProviders(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var root = configuration
            .GetSection(ExternalProvidersOptions.SectionName)
            .Get<ExternalProvidersOptions>() ?? new ExternalProvidersOptions();

        foreach (var (provider, settings) in root.Providers)
        {
            if (!settings.IsConfigured || !ExternalProvidersOptions.KnownProviders.ContainsKey(provider))
            {
                continue;
            }

            builder.AddExternalProvider(provider, settings);
        }

        return builder;
    }

    private static AuthenticationBuilder AddExternalProvider(
        this AuthenticationBuilder builder,
        string provider,
        ExternalProviderOptions settings)
    {
        var clientId = settings.ClientId!;
        var clientSecret = settings.ClientSecret!;
        var callbackPath = ExternalAccountEndpoints.CallbackPathFor(provider);
        var surRetour = ExternalSignInTicketEvents.ForProvider(provider);

        return provider.ToUpperInvariant() switch
        {
            "GOOGLE" => builder.AddGoogle(ExternalSchemes.Google, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
                options.AccessType = "offline";
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // La certification de l'adresse est une revendication du jeton. Le gestionnaire
                // ne la transmet pas sans instruction explicite, et sans elle aucun
                // rattachement automatique n'est possible.
                options.ClaimActions.MapJsonKey("email_verified", "email_verified");
                options.Events.OnTicketReceived = surRetour;
            }),

            "MICROSOFT" => AddMicrosoft(builder, settings, callbackPath, surRetour),

            "GITHUB" => builder.AddGitHub(ExternalSchemes.GitHub, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
                options.Scope.Clear();
                options.Scope.Add("read:user");

                // L'adresse n'est lue qu'avec cette portée. GitHub ne publie pas de
                // certification d'adresse dans son jeton : le rattachement automatique reste
                // impossible et le membre doit définir un mot de passe.
                options.Scope.Add("user:email");
                options.ClaimActions.MapJsonKey("name", "name", ClaimValueTypes.String);
                options.Events.OnTicketReceived = surRetour;
            }),

            _ => builder,
        };
    }

    private static AuthenticationBuilder AddMicrosoft(
        AuthenticationBuilder builder,
        ExternalProviderOptions settings,
        PathString callbackPath,
        Func<Microsoft.AspNetCore.Authentication.TicketReceivedContext, Task> surRetour)
    {
        var clientId = settings.ClientId!;
        var clientSecret = settings.ClientSecret!;
        var tenant = string.IsNullOrWhiteSpace(settings.Tenant) ? "common" : settings.Tenant.Trim();
        var isOrganisation = !tenant.Equals("common", StringComparison.OrdinalIgnoreCase);

        if (!isOrganisation)
        {
            return builder.AddMicrosoftAccount(ExternalSchemes.Microsoft, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.ClaimActions.MapJsonKey("email_verified", "email_verified");
                options.Events.OnTicketReceived = surRetour;
            });
        }

        // Un compte professionnel doit être validé par le jeton du locataire : c'est la
        // seule façon d'interdire à un compte personnel d'accéder à un espace privé.
        return builder.AddOpenIdConnect(ExternalSchemes.MicrosoftOrganisational, options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = callbackPath;
            options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ClaimActions.MapJsonKey("email_verified", "email_verified");
            options.TokenValidationParameters.ValidateIssuer = true;
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.Events.OnTicketReceived = surRetour;
        });
    }
}
