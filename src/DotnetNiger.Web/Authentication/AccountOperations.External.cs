using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Parcours d'un membre arrivé par un fournisseur externe : session, rattachement, création.
/// </summary>
/// <remarks>
/// Ces opérations partagent le même modèle de confiance : le fournisseur a certifié
/// l'adresse, pas le mot de passe. Tout ce qui exige un mot de passe passe donc par
/// <see cref="SignInManager{TUser}"/>, seul à même d'appliquer le verrouillage.
/// </remarks>
public sealed partial class AccountOperations
{
    /// <summary>
    /// Ouvre une session pour un compte dont l'adresse a été certifiée par un fournisseur.
    /// </summary>
    /// <param name="user">Compte à connecter.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Résultat de la connexion.</returns>
    public async Task SignInVerifiedUserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Le compte est utilisable sans confirmation d'adresse : le fournisseur a certifié
        // que la personne contrôle cette adresse. `RequireConfirmedEmail` ne concernait que
        // la connexion par mot de passe, qui passe bien par PasswordSignInAsync.
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        await signInManager.SignInAsync(user, isPersistent: false);
    }

    /// <summary>
    /// Rattachement d'une identité externe à un compte existant, après saisie du mot de passe.
    /// </summary>
    /// <param name="pending">Identité externe en attente.</param>
    /// <param name="password">Mot de passe saisi par le membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Résultat de l'opération.</returns>
    public async Task<PasswordProofResult> LinkWithPasswordAsync(
        PendingExternalSignIn pending,
        string password,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pending);

        if (string.IsNullOrWhiteSpace(pending.Email))
        {
            return PasswordProofResult.Failure("Aucun compte ne correspond à cette identité.");
        }

        var account = await userManager.FindByEmailAsync(pending.Email.Trim());

        if (account is null)
        {
            return PasswordProofResult.Failure("Aucun compte ne correspond à cette identité.");
        }

        // Le mot de passe est vérifié par SignInManager : c'est lui qui applique le
        // verrouillage après trop d'échecs. Refaire la comparaison ici, sans ce compteur,
        // laisserait un formulaire deviner le mot de passe par des tentatives illimitées.
        var check = await signInManager.CheckPasswordSignInAsync(
            account,
            password,
            lockoutOnFailure: true);

        if (check.Succeeded)
        {
            var link = await userManager.AddLoginAsync(
                account,
                new UserLoginInfo(pending.Provider, pending.ProviderKey, pending.Provider));

            return link.Succeeded
                ? PasswordProofResult.Success(account)
                : PasswordProofResult.Failure(
                    string.Join(" ", link.Errors.Select(erreur => erreur.Description)));
        }

        if (check.IsLockedOut)
        {
            return PasswordProofResult.Failure("Ce compte est temporairement verrouillé.");
        }

        if (check.IsNotAllowed)
        {
            return PasswordProofResult.Failure("Confirmez d'abord votre adresse e-mail.");
        }

        return PasswordProofResult.Failure("Mot de passe incorrect.");
    }

    /// <summary>
    /// Crée le compte d'un membre arrivé par un fournisseur externe et y rattache l'identité.
    /// </summary>
    /// <param name="pending">Identité externe en attente.</param>
    /// <param name="input">Mot de passe choisi.</param>
    /// <param name="externalAccounts">Service de rattachement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Résultat de la création.</returns>
    public async Task<ExternalAccountResult> CreateFromExternalAsync(
        PendingExternalSignIn pending,
        string input,
        IExternalAccountService externalAccounts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pending);
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(pending.Email))
        {
            return ExternalAccountResult.Failure(
                "Le fournisseur n'a pas transmis d'adresse e-mail : utilisez un mot de passe local.");
        }

        var email = pending.Email.Trim();
        var user = new ApplicationUser(email);

        // Le nom affiché renvoyé par le fournisseur n'est pas recopié : un prénom composé ou
        // un nom de marque se découpent mal, et une valeur inventée se retrouverait sur les
        // pages publiques. Le membre complète son profil après sa première connexion.
        //
        // La photo, elle, est reprise telle quelle : elle est déjà publique chez le
        // fournisseur, et c'est le seul moyen d'avoir un avatar dès la première connexion
        // sans que le membre aille le chercher. Elle n'est appliquée qu'après la création
        // réussie, pour ne pas laisser de photo sur un compte qui n'a pas abouti.

        var result = await externalAccounts.CreateWithPasswordAsync(
            user,
            input,
            pending.Provider,
            pending.ProviderKey,
            pending.Provider);

        if (!result.Succeeded)
        {
            return ExternalAccountResult.Failure(
                string.Join(" ", result.Errors.Select(erreur => erreur.Description)));
        }

        if (!string.IsNullOrWhiteSpace(pending.PhotoUrl))
        {
            user.SetProfilePhoto(pending.PhotoUrl);
            await userManager.UpdateAsync(user);
        }

        // L'adresse n'est confirmée que si le fournisseur l'a certifiée. Sinon elle part par
        // email comme pour une inscription locale, car personne n'a encore prouvé qu'elle
        // appartient au membre.
        if (pending.EmailVerified)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            await TrySendAsync(
                () => SendExternalWelcomeAsync(user, cancellationToken),
                user.Email!);
        }
        else
        {
            await TrySendAsync(
                () => SendRegistrationEmailsAsync(user, cancellationToken),
                user.Email!);
        }

        return ExternalAccountResult.Success(user, pending.EmailVerified);
    }
}