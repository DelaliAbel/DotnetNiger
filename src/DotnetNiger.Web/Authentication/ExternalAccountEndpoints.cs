using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Points de terminaison du parcours de connexion par fournisseur externe.
/// </summary>
public static class ExternalAccountEndpoints
{
    /// <summary>
    /// Déclare les routes de connexion externe.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    public static WebApplication MapExternalAccountEndpoints(this WebApplication app)
    {
        // Démarrage d'une connexion externe. La requête n'est protégée par aucun jeton :
        // c'est une page publique, et le fournisseur prend le relais immédiatement.
        app.MapGet("/connexion/{provider}", (
            string provider,
            HttpContext context,
            IConfiguration configuration) =>
        {
            var name = provider.Trim();

            if (!TryGetSettings(configuration, name, out var settings))
            {
                // Un fournisseur absent n'est pas une erreur technique : le bouton ne doit
                // pas exister. La réponse évite de révéler quels fournisseurs sont activés.
                return Results.NotFound();
            }

            // Un défi, et non une redirection : c'est la réponse d'authentification qui
            // déclenche la fenêtre du fournisseur. La destination demandée est conservée
            // dans les propriétés, car l'adresse de retour appartient désormais au
            // fournisseur.
            var properties = ExternalSignInNavigation.BuildChallengeProperties(
                context.Request.Query["returnUrl"].ToString(),
                CallbackPathFor(name));

            return Results.Challenge(properties, [ExternalSchemes.ForProvider(name, settings.Tenant)]);
        })
        .AllowAnonymous()
        .ExcludeFromDescription();

        return app;
    }

    /// <summary>
    /// Adresse de retour déclarée chez le fournisseur pour une connexion donnée.
    /// </summary>
    /// <param name="provider">Nom de configuration du fournisseur.</param>
    /// <returns>Chemin de retour.</returns>
    public static string CallbackPathFor(string provider) =>
        $"/connexion/{provider.ToLowerInvariant()}/retour";

    private static bool TryGetSettings(
        IConfiguration configuration,
        string provider,
        out ExternalProviderOptions settings)
    {
        settings = default!;

        if (!ExternalProvidersOptions.KnownProviders.ContainsKey(provider))
        {
            return false;
        }

        var root = configuration
            .GetSection(ExternalProvidersOptions.SectionName)
            .Get<ExternalProvidersOptions>();

        return root is not null
            && root.Providers.TryGetValue(provider, out var found)
            && found.IsConfigured
            && (settings = found) is not null;
    }
}
