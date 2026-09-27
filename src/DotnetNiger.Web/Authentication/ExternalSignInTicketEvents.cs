using Microsoft.AspNetCore.Authentication;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Traitement du retour d'un fournisseur d'identité, branché sur chaque fournisseur
/// enregistré.
/// </summary>
/// <remarks>
/// Le retour du fournisseur ne peut pas être traité par un point de terminaison d'API
/// minimale : le gestionnaire d'authentification distante absorbe l'adresse de retour
/// déclarée chez le fournisseur et y répond lui-même, avant que le routeur ne soit
/// consulté. Le traitement est donc fait dans <c>OnTicketReceived</c>, où l'identité
/// revenue est disponible et où la réponse par défaut peut être remplacée.
/// </remarks>
public static class ExternalSignInTicketEvents
{
    /// <summary>
    /// Construit le gestionnaire d'événement de retour d'un fournisseur.
    /// </summary>
    /// <param name="provider">Nom de configuration du fournisseur.</param>
    /// <returns>Gestionnaire à brancher sur les options du fournisseur.</returns>
    public static Func<TicketReceivedContext, Task> ForProvider(string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        return async context =>
        {
            var services = context.HttpContext.RequestServices;
            var handler = services.GetRequiredService<ExternalCallbackHandler>();
            var protector = services.GetRequiredService<PendingExternalSignInProtector>();

            var returnUrl = ExternalSignInNavigation.ReadReturnUrl(context.Properties);

            string destination;

            try
            {
                var outcome = await handler.HandleAsync(
                    context.HttpContext,
                    provider,
                    context.Principal!,
                    returnUrl);

                destination = outcome.Status == ExternalCallbackStatus.SignedIn
                    ? outcome.RedirectUrl!
                    : BuildPasswordUrl(outcome, returnUrl, protector);
            }
            catch (ExternalIdentityException)
            {
                destination = "/compte/connexion?erreur=identite";
            }

            // Sans cet appel, le gestionnaire connecterait la session temporaire du
            // fournisseur et renverrait le visiteur à la racine, en ignorant le choix du mot
            // de passe et la destination demandée.
            context.HandleResponse();
            context.Response.Redirect(destination);
        };
    }

    /// <summary>
    /// Compose l'adresse de la page de choix du mot de passe.
    /// </summary>
    /// <param name="outcome">Issue du traitement de l'identité.</param>
    /// <param name="returnUrl">Destination interne demandée avant le départ.</param>
    /// <param name="protector">Protecteur du jeton de l'identité en attente.</param>
    /// <returns>Adresse de la page, jeton protégé en paramètre.</returns>
    private static string BuildPasswordUrl(
        ExternalCallbackOutcome outcome,
        string? returnUrl,
        PendingExternalSignInProtector protector)
    {
        // L'identité du fournisseur ne transite pas en clair dans l'adresse : elle est
        // transportée par un jeton protégé, illisible et infalsifiable hors du site.
        var pending = new PendingExternalSignIn(
            outcome.Provider!,
            outcome.ProviderKey!,
            outcome.Email,
            outcome.EmailVerified,
            returnUrl);

        var jeton = Uri.EscapeDataString(protector.Protect(pending));

        return outcome.RequiresExistingPassword
            ? $"/compte/mot-de-passe?jeton={jeton}&preuve=1"
            : $"/compte/mot-de-passe?jeton={jeton}";
    }
}
