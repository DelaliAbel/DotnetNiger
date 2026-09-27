using DotnetNiger.Application.Common;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Infrastructure.Email;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Points de terminaison du parcours de compte par mot de passe.
/// </summary>
public static class AccountEndpoints
{
    /// <summary>
    /// Déclare les routes d'inscription, de connexion et de déconnexion.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    /// <remarks>
    /// Toutes ces opérations modifient l'état du compte ou de la session : elles passent
    /// par POST et restent visées par la protection contre la falsification de requête, dont
    /// les jetons sont insérés par le composant <c>AntiforgeryToken</c> des pages. Un
    /// formulaire de connexion ne doit jamais être rendu en GET : le mot de passe se
    /// retrouverait dans l'historique du navigateur et dans les journaux du serveur.
    /// </remarks>
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/compte/connexion/se-connecter", async (
            [FromForm] LoginInput input,
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            CancellationToken cancellationToken) =>
        {
            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                return Results.Redirect($"/compte/connexion?erreur={Uri.EscapeDataString(erreur)}");
            }

            var result = await signInManager.PasswordSignInAsync(
                input.Email.Trim(),
                input.Password,
                input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // La destination est transmise par le champ caché du formulaire, la query
                // faisant office de repli pour les liens qui la transporting déjà.
                var retour = string.IsNullOrWhiteSpace(input.Retour)
                    ? context.Request.Query["retour"].ToString()
                    : input.Retour;

                return Results.Redirect(SafeRedirect(retour));
            }

            var motif = result.IsNotAllowed
                ? "Confirmez d'abord votre adresse e-mail."
                : result.IsLockedOut
                    ? "Trop de tentatives. Réessayez dans quelques minutes."
                    : "Adresse e-mail ou mot de passe incorrect.";

            return Results.Redirect($"/compte/connexion?erreur={Uri.EscapeDataString(motif)}");
        })
        .AllowAnonymous();

        app.MapPost("/compte/inscription/creer", async (
            [FromForm] RegisterInput input,
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            AccountOperations accounts,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                AccountLog.InscriptionRefusee(logger, erreur);

                return Results.Redirect(
                    $"/compte/inscription?erreur={Uri.EscapeDataString(erreur)}");
            }

            var user = new ApplicationUser(input.Email.Trim());
            user.UpdateProfile(input.FirstName.Trim(), input.LastName.Trim());

            var creation = await userManager.CreateAsync(user, input.Password);

            if (!creation.Succeeded)
            {
                var motif = creation.Errors
                    .Select(erreur => erreur.Description)
                    .Aggregate((gauche, droite) => $"{gauche} {droite}");

                AccountLog.InscriptionRefusee(logger, motif);

                return Results.Redirect(
                    $"/compte/inscription?erreur={Uri.EscapeDataString(motif)}");
            }

            await userManager.AddToRoleAsync(user, RoleNames.Member);
            AccountLog.CompteCree(logger, user.Email!);

            try
            {
                await accounts.SendRegistrationEmailsAsync(user, cancellationToken);
            }
            catch (EmailDeliveryException)
            {
                // Le compte existe, mais le courriel de confirmation n'est pas parti. Échouer
                // franchement laisserait un compte que personne ne peut activer : mieux vaut
                // prévenir le membre et lui proposer de renvoyer le message.
                AccountLog.CourrielConfirmationEchoue(logger, user.Email!);

                return Results.Redirect(
                    $"/compte/inscription?courriel={Uri.EscapeDataString(user.Email!)}&courrielEchoue=1");
            }

            return Results.Redirect(
                "/compte/inscription?envoye=1&courriel="
                + Uri.EscapeDataString(user.Email!));
        })
        .AllowAnonymous();

        app.MapPost("/compte/deconnexion", async (
            SignInManager<ApplicationUser> signInManager,
            HttpContext context) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        })
        .RequireAuthorization();

        return app;
    }

    private static string SafeRedirect(string? url) =>
        ExternalSignInNavigation.IsLocal(url) ? url! : "/";
}
