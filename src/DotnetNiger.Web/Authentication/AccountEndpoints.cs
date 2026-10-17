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

                return Results.Redirect(DestinationConnexion.Pour(retour, await EstAdministrateurAsync(input.Email, signInManager, cancellationToken)));
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

                return Results.Redirect(InscriptionRefusee(input, erreur));
            }

            var user = new ApplicationUser(input.Email.Trim());

            // Pas de fonction à l'inscription : elle est saisie plus tard par
            // l'administration, au moment où elle inscrit le membre dans l'équipe.
            user.UpdateProfile(input.FirstName.Trim(), input.LastName.Trim(), profession: null);

            var creation = await userManager.CreateAsync(user, input.Password);

            if (!creation.Succeeded)
            {
                var motif = creation.Errors
                    .Select(erreur => erreur.Description)
                    .Aggregate((gauche, droite) => $"{gauche} {droite}");

                AccountLog.InscriptionRefusee(logger, motif);

                return Results.Redirect(InscriptionRefusee(input, motif));
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

    /// <summary>
    /// Renvoie vers le formulaire d'inscription en conservant ce qui a été saisi.
    /// </summary>
    /// <param name="input">Saisie du formulaire.</param>
    /// <param name="message">Motif du refus.</param>
    /// <returns>L'adresse de retour, motif et nom compris.</returns>
    /// <remarks>
    /// Revenir sur une page vide obligerait à retaper le prénom, le nom et l'adresse
    /// alors qu'une seule valeur est en cause. Le mot de passe n'est jamais renvoyé :
    /// le faire circuler dans l'adresse serait l'exposer à l'historique du navigateur.
    /// </remarks>
    private static string InscriptionRefusee(RegisterInput input, string message)
    {
        var retour =
            $"/compte/inscription?erreur={Uri.EscapeDataString(message)}"
            + $"&prenom={Uri.EscapeDataString(input.FirstName ?? string.Empty)}"
            + $"&nom={Uri.EscapeDataString(input.LastName ?? string.Empty)}"
            + $"&courrielSaisi={Uri.EscapeDataString(input.Email ?? string.Empty)}";

        return retour;
    }

    /// <summary>
    /// Indique si le compte qui vient de se connecter porte le rôle administrateur.
    /// </summary>
    /// <remarks>
    /// Le compte est relu après le succès de la connexion : le cookie vient d'être émis,
    /// mais <c>HttpContext.User</c> décrit encore le visiteur d'avant la connexion
    /// jusqu'à la requête suivante. Sans cette relecture, un administrateur partirait vers
    /// le tableau de bord membre.
    /// </remarks>
    /// <param name="email">Adresse saisie dans le formulaire.</param>
    /// <param name="signInManager">Gestionnaire d'identité de la requête.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si le compte est administrateur.</returns>
    private static async Task<bool> EstAdministrateurAsync(
        string? email,
        SignInManager<ApplicationUser> signInManager,
        CancellationToken cancellationToken)
    {
        var compte = await signInManager.UserManager
            .FindByEmailAsync(email?.Trim() ?? string.Empty);

        return compte is not null
            && await signInManager.UserManager.IsInRoleAsync(compte, RoleNames.Admin);
    }
}
