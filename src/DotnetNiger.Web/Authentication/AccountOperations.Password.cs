using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Mot de passe oublié : composer le lien, puis enregistrer le nouveau mot de passe.
/// </summary>
public sealed partial class AccountOperations
{
    /// <summary>
    /// Compose et envoie le lien de réinitialisation d'un mot de passe oublié.
    /// </summary>
    /// <param name="user">Compte ayant demandé un nouveau mot de passe.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche complète une fois le message remis à l'expéditeur.</returns>
    /// <exception cref="EmailDeliveryException">Le message n'a pas pu être remis.</exception>
    public async Task SendPasswordResetAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Sans adresse du site, aucun lien ne peut être composé : le jeton serait généré puis
        // perdu. Le message est journalisé, et l'appelant — qui a consulté
        // <see cref="EmailConfigured"/> — annonce l'absence d'envoi au membre.
        if (!_email.IsConfigured)
        {
            LogCourrielNonConfigure(logger, user.Email!);
            return;
        }

        var jeton = await userManager.GeneratePasswordResetTokenAsync(user);

        var url = _email.BuildUrl(
            $"{ResetPasswordUrl}?courriel={Uri.EscapeDataString(user.Email!)}"
            + $"&jeton={Uri.EscapeDataString(jeton)}");

        await emails.SendAsync(
            user.Email!,
            EmailTemplateCatalog
                .Obtenir(EmailTemplateKeys.AccountPasswordReset)
                .Rendre(
                    EmailValues.De(
                        (EmailVariables.NomMembre, user.FullName ?? string.Empty),
                        (EmailVariables.UrlReinitialisation, url)),
                    EmailBranding.DepuisOptions(_email)),
            cancellationToken);
    }

    /// <summary>
    /// Remplace le mot de passe d'un compte à partir d'un jeton de réinitialisation.
    /// </summary>
    /// <param name="email">Adresse portée par le lien.</param>
    /// <param name="token">Jeton de réinitialisation.</param>
    /// <param name="password">Nouveau mot de passe.</param>
    /// <returns>Résultat de l'opération.</returns>
    public async Task<PasswordResetResult> ResetPasswordAsync(
        string email,
        string token,
        string password)
    {
        var account = await userManager.FindByEmailAsync(email.Trim());

        // Un jeton peut être valide pour un compte supprimé depuis l'envoi. Le message reste
        // volontairement identique à celui d'un jeton expiré : la différence entre les deux cas
        // ne doit rien apprendre sur l'existence d'un compte.
        if (account is null)
        {
            return PasswordResetResult.Failure("Ce lien a expiré ou a déjà été utilisé.");
        }

        var result = await userManager.ResetPasswordAsync(account, token, password);

        if (result.Succeeded)
        {
            return PasswordResetResult.Success(account);
        }

        return PasswordResetResult.Failure(
            string.Join(" ", result.Errors.Select(erreur => erreur.Description)));
    }
}