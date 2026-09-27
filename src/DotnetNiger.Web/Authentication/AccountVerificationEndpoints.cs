using System.ComponentModel.DataAnnotations;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Points de terminaison de confirmation d'adresse et de choix de mot de passe.
/// </summary>
public static class AccountVerificationEndpoints
{
    /// <summary>
    /// Déclare les routes de fin du parcours d'inscription.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    public static WebApplication MapAccountVerificationEndpoints(this WebApplication app)
    {
        // Confirmation de l'adresse après inscription locale. Le lien arrive par courriel :
        // il n'est accessible qu'à la personne qui a reçu le message.
        app.MapGet("/compte/confirmer-email", async (
            string? jeton,
            string? courriel,
            UserManager<ApplicationUser> userManager,
            AccountOperations accounts,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(jeton) || string.IsNullOrWhiteSpace(courriel))
            {
                return Results.Redirect("/compte/connexion?erreur=confirmation");
            }

            var user = await userManager.FindByEmailAsync(courriel);

            if (user is null)
            {
                return Results.Redirect("/compte/connexion?erreur=confirmation");
            }

            var result = await userManager.ConfirmEmailAsync(user, jeton);

            if (!result.Succeeded)
            {
                // Un lien de confirmation peut être cliqué deux fois, ou expiré. Ce n'est pas
                // une raison d'afficher une page d'erreur : le compte est déjà utilisable.
                return Results.Redirect("/compte/connexion?confirme=1");
            }

            try
            {
                await accounts.SendWelcomeAsync(user, cancellationToken);
            }
            catch (EmailDeliveryException)
            {
                // L'adresse est confirmée et le compte utilisable. Le courriel de bienvenue
                // est secondary : son absence ne doit pas invalider la confirmation.
                AccountLog.CourrielConfirmationEchoue(logger, user.Email!);
            }

            return Results.Redirect("/compte/connexion?confirme=1");
        })
        .AllowAnonymous();

        // Choix du mot de passe après un retour de fournisseur externe. L'identité du
        // fournisseur arrive dans un jeton protégé, jamais en clair dans l'URL.
        app.MapPost("/compte/mot-de-passe/enregistrer", async (
            [FromForm] SetPasswordInput input,
            PendingExternalSignInProtector protector,
            AccountOperations accounts,
            IExternalAccountService externalAccounts,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var pending = protector.Unprotect(input.Token);

            if (pending is null)
            {
                return Results.Redirect("/compte/connexion?erreur=session");
            }

            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                AccountLog.CompteExterneRefuse(logger, pending.Email ?? "(aucune adresse)");

                return Results.Redirect(
                    $"/compte/mot-de-passe?jeton={Uri.EscapeDataString(input.Token)}"
                    + $"&erreur={Uri.EscapeDataString(erreur)}");
            }

            // Le cas « un compte existe déjà pour cette adresse » est traité à part : il faut
            // prouver le compte existant, pas en créer un second sous la même adresse.
            if (await ExistingAccountAsync(userManager, pending) is not null)
            {
                return Results.Redirect(
                    $"/compte/mot-de-passe?jeton={Uri.EscapeDataString(input.Token)}&preuve=1");
            }

            var creation = await accounts.CreateFromExternalAsync(
                pending,
                input.Password,
                externalAccounts,
                cancellationToken);

            if (!creation.IsSuccess)
            {
                AccountLog.CompteExterneRefuse(logger, pending.Email ?? "(aucune adresse)");

                return Results.Redirect(
                    $"/compte/mot-de-passe?jeton={Uri.EscapeDataString(input.Token)}"
                    + $"&erreur={Uri.EscapeDataString(creation.Error ?? "Création impossible.")}");
            }

            AccountLog.MotDePasseExterneDefini(logger, creation.User!.Email!);

            // L'adresse étant certifiée, le compte est utilisable immédiatement. Sinon il
            // reste en attente de confirmation, et aucune session n'est ouverte.
            if (pending.EmailVerified)
            {
                await accounts.SignInVerifiedUserAsync(creation.User, cancellationToken);

                return Results.Redirect(
                    ExternalSignInNavigation.IsLocal(pending.ReturnUrl) ? pending.ReturnUrl! : "/");
            }

            return Results.Redirect(
                "/compte/connexion?envoye=1&courriel=" + Uri.EscapeDataString(creation.User.Email!));
        })
        .AllowAnonymous();

        // Preuve de propriété : le membre saisit le mot de passe du compte existant pour
        // rattacher son identité externe.
        app.MapPost("/compte/mot-de-passe/preuve", async (
            [FromForm] ProofInput input,
            PendingExternalSignInProtector protector,
            AccountOperations accounts,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var pending = protector.Unprotect(input.Token);

            if (pending is null)
            {
                return Results.Redirect("/compte/connexion?erreur=session");
            }

            if (AccountInputValidation.Validate(input) is { } erreur)
            {
                AccountLog.PreuveRefusee(logger, pending.Email ?? "(aucune adresse)");

                return Results.Redirect(
                    $"/compte/mot-de-passe?jeton={Uri.EscapeDataString(input.Token)}"
                    + $"&preuve=1&erreur={Uri.EscapeDataString(erreur)}");
            }

            var proof = await accounts.LinkWithPasswordAsync(pending, input.Password, cancellationToken);

            if (!proof.IsSuccess)
            {
                AccountLog.PreuveRefusee(logger, pending.Email ?? "(aucune adresse)");

                return Results.Redirect(
                    $"/compte/mot-de-passe?jeton={Uri.EscapeDataString(input.Token)}"
                    + $"&preuve=1&erreur={Uri.EscapeDataString(proof.Error ?? "Saisie refusée.")}");
            }

            await signInManager.SignInAsync(proof.User!, isPersistent: false);

            return Results.Redirect(
                ExternalSignInNavigation.IsLocal(pending.ReturnUrl) ? pending.ReturnUrl! : "/");
        })
        .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Compte occupant déjà l'adresse, si le fournisseur n'y est pas encore rattaché.
    /// </summary>
    /// <param name="userManager">Gestionnaire des comptes.</param>
    /// <param name="pending">Identité externe en attente.</param>
    /// <returns>Le compte à prouver, ou null si rien n'est en conflit.</returns>
    private static async Task<ApplicationUser?> ExistingAccountAsync(
        UserManager<ApplicationUser> userManager,
        PendingExternalSignIn pending)
    {
        if (string.IsNullOrWhiteSpace(pending.Email))
        {
            return null;
        }

        var account = await userManager.FindByEmailAsync(pending.Email.Trim());

        if (account is null)
        {
            return null;
        }

        // L'identité est peut-être déjà rattachée à ce compte, auquel cas il n'y a rien à
        // prouver : le rattachement a simplement échoué au retour du fournisseur.
        var dejaLie = await userManager.FindByLoginAsync(pending.Provider, pending.ProviderKey);

        return ReferenceEquals(dejaLie, account) ? null : account;
    }
}

/// <summary>
/// Formulaire de preuve de propriété d'un compte.
/// </summary>
public sealed class ProofInput
{
    /// <summary>
    /// Jeton protégé portant l'identité externe en attente.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe du compte existant.
    /// </summary>
    [Required(ErrorMessage = "Saisissez votre mot de passe actuel.")]
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
