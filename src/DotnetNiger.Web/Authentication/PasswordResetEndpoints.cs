using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Points de terminaison du parcours « mot de passe oublié ».
/// </summary>
public static class PasswordResetEndpoints
{
    /// <summary>
    /// Déclare la demande d'envoi et la validation d'un nouveau mot de passe.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    /// <remarks>
    /// Les deux opérations passent par POST : le nouveau mot de passe ne doit jamais figurer
    /// dans une URL, donc dans l'historique du navigateur ni dans les journaux du serveur. Le
    /// jeton de réinitialisation est la seule donnée réellement secrète, et il sert une fois.
    /// </remarks>
    public static WebApplication MapPasswordResetEndpoints(this WebApplication app)
    {
        // Un formulaire ne se soumet qu'une fois, mais rien n'interdit de rejouer la requête.
        // Le compteur de limitation de débit, posé en amont dans le pipeline, borne ce cas
        // comme tous les autres envois répétés.
        app.MapPost("/compte/mot-de-passe-oublie/demander", async (
            [FromForm] ForgotPasswordInput input,
            UserManager<ApplicationUser> userManager,
            AccountOperations accounts,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                return Results.Redirect(
                    $"/compte/mot-de-passe-oublie?erreur={Uri.EscapeDataString(erreur)}");
            }

            var email = input.Email.Trim();
            var account = await userManager.FindByEmailAsync(email);

            // Le parcours renvoie le même message dans tous les cas. Répondre « compte
            // inconnu » permettrait de dresser la liste des membres inscrits à partir de ce seul
            // formulaire, ce que la page d'inscription ne permet pas de façon exploitable.
            // L'absence de serveur de courriels, elle, se signale quel que soit le compte :
            // c'est une propriété de l'instance, pas une information sur le demandeur.
            if (account is null)
            {
                AccountLog.ReinitialisationDemandeeInconnue(logger, email);

                return Results.Redirect(
                    PasswordResetRedirect.Confirmation(email, accounts.EmailConfigured));
            }

            if (!accounts.EmailConfigured)
            {
                // Aucun serveur de courriel en développement : annoncer un envoi qui n'a pas
                // eu lieu laisserait le membre sans rien attendre.
                AccountLog.CourrielReinitialisationNonConfigure(logger, email);

                return Results.Redirect(PasswordResetRedirect.Confirmation(email, emailConfigure: false));
            }

            try
            {
                await accounts.SendPasswordResetAsync(account, cancellationToken);
                AccountLog.ReinitialisationEnvoyee(logger, email);
            }
            catch (EmailDeliveryException)
            {
                // Impossible d'annoncer l'échec à cette personne : la page le ferait pour ce seul
                // compte, et l'anomalie se lirait dans la réponse. Le journal (événement 49) et
                // la supervision la signalent, elle est réservée à l'exploitation.
                AccountLog.CourrielReinitialisationEchoue(logger, email);
            }

            return Results.Redirect(PasswordResetRedirect.Confirmation(email, emailConfigure: true));
        })
        .AllowAnonymous();

        app.MapPost("/compte/reinitialiser-mot-de-passe", async (
            [FromForm] ResetPasswordInput input,
            AccountOperations accounts,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            // L'adresse voyage dans un champ caché : elle disparaît de la query à la
            // soumission, donc de l'historique du navigateur et des journaux du serveur.
            var email = input.Email.Trim();

            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                AccountLog.ReinitialisationRefusee(logger, email);

                return Results.Redirect(ResetBack(input.Email, input.Token, erreur));
            }

            var result = await accounts.ResetPasswordAsync(email, input.Token, input.Password);

            if (!result.IsSuccess)
            {
                AccountLog.ReinitialisationRefusee(logger, email);

                return Results.Redirect(ResetBack(email, input.Token, result.Error));
            }

            // Le mot de passe ayant changé, la clé de session ne vaut plus : une session
            // ouverte avant le changement doit être invalidated, sinon un mot de passe
            // réinitialisé après une intrusion laisserait l'intrus connecté.
            await signInManager.SignOutAsync();

            AccountLog.MotDePasseReinitialise(logger, result.User!.Email!);

            return Results.Redirect("/compte/connexion?confirme=motdepasse");
        })
        .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Redirection vers le formulaire de nouveau mot de passe, motif d'échec compris.
    /// </summary>
    /// <param name="email">Adresse du compte, à replacer dans le formulaire.</param>
    /// <param name="token">Jeton à conserver pour permettre un nouvel essai.</param>
    /// <param name="motif">Message d'échec.</param>
    /// <returns>Redirection vers la page de réinitialisation.</returns>
    private static string ResetBack(string email, string token, string? motif)
    {
        var raison = motif ?? "Réinitialisation impossible.";

        return $"{AccountOperations.ResetPasswordUrl}?erreur={Uri.EscapeDataString(raison)}"
            + $"&courriel={Uri.EscapeDataString(email)}"
            + $"&jeton={Uri.EscapeDataString(token)}";
    }
}
