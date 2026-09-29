using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Opérations de compte partagées par les pages et les points de terminaison.
/// </summary>
public sealed partial class AccountOperations(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender emails,
    IOptions<EmailOptions> emailOptions,
    ILogger<AccountOperations> logger)
{
    private readonly EmailOptions _email = emailOptions.Value;

    /// <summary>
    /// Adresse de la page d'accueil du parcours compte.
    /// </summary>
    public static string CommunityUrl { get; } = "/communaute";

    /// <summary>
    /// Adresse de la page qui saisit un nouveau mot de passe.
    /// </summary>
    /// <remarks>
    /// Le lien porte l'adresse et le jeton dans la query : la page se rend en statique, donc
    /// le formulaire n'a pas d'état serveur où retrouver le compte. L'adresse n'est qu'un
    /// indice de recherche — seul le jeton, validé par Identity, autorise le changement.
    /// </remarks>
    public static string ResetPasswordUrl { get; } = "/compte/reinitialiser-mot-de-passe";

    /// <summary>
    /// Vrai si l'envoi de courriel est opérationnel.
    /// </summary>
    /// <remarks>
    /// En développement local, aucun serveur SMTP n'est configuré. Le parcours doit alors
    /// le dire plutôt que d'affirmer un envoi qui n'a pas eu lieu.
    /// </remarks>
    public bool EmailConfigured => _email.IsConfigured;

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

    /// <summary>
    /// Envoie le courriel de confirmation et de bienvenue après une inscription locale.
    /// </summary>
    /// <param name="user">Compte créé.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche complète une fois les messages remis à l'expéditeur.</returns>
    /// <remarks>
    /// L'ordre des deux messages est celui demandé pour une inscription locale : d'abord la
    /// confirmation, puis la bienvenue une fois l'adresse validée. La bienvenue part de
    /// l'inscription parce que c'est le seul moment où l'on sait que l'adresse est la bonne
    /// ; elle est renvoyée à la confirmation.
    /// </remarks>
    public async Task SendRegistrationEmailsAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Le lien exige une adresse de site. En développement local, aucun serveur SMTP
        // n'est configuré : composer l'URL ici ferait échouer l'inscription avec une erreur
        // technique alors que le compte vient d'être créé. On journalise et on laisse le
        // membre confirmer son adresse depuis un nouvel envoi.
        if (!_email.IsConfigured)
        {
            LogCourrielNonConfigure(logger, user.Email!);
            return;
        }

        var jeton = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var url = _email.BuildUrl(
            $"/compte/confirmer-email?jeton={Uri.EscapeDataString(jeton)}&courriel={Uri.EscapeDataString(user.Email!)}");

        await emails.SendAsync(
            user.Email!,
            EmailTemplateCatalog
                .Obtenir(EmailTemplateKeys.AccountConfirmation)
                .Rendre(
                    EmailValues.De(
                        (EmailVariables.NomMembre, user.FullName ?? string.Empty),
                        (EmailVariables.UrlConfirmation, url)),
                    EmailBranding.DepuisOptions(_email)),
            cancellationToken);
    }

    /// <summary>
    /// Envoie le courriel de bienvenue après validation de l'adresse.
    /// </summary>
    /// <param name="user">Compte confirmé.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche complète une fois le message remis à l'expéditeur.</returns>
    public async Task SendWelcomeAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        await emails.SendAsync(
            user.Email!,
            EmailTemplateCatalog
                .Obtenir(EmailTemplateKeys.AccountWelcome)
                .Rendre(
                    EmailValues.De(
                        (EmailVariables.NomMembre, user.FullName ?? string.Empty),
                        (EmailVariables.UrlCompte, _email.BuildUrl(CommunityUrl))),
                    EmailBranding.DepuisOptions(_email)),
            cancellationToken);
    }

    /// <summary>
    /// Envoie le courriel de bienvenue à un membre arrivé par un fournisseur externe dont
    /// l'adresse a été certifiée.
    /// </summary>
    /// <param name="user">Compte créé.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche complète une fois le message remis à l'expéditeur.</returns>
    /// <remarks>
    /// Le mot de passe est déjà choisi à ce stade : le message de bienvenue ne propose donc
    /// pas d'en définir un. Un lien « définir mon mot de passe » sans jeton mènerait à une
    /// page qui ne pourrait rien enregistrer.
    /// </remarks>
    public async Task SendExternalWelcomeAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        await emails.SendAsync(
            user.Email!,
            EmailTemplateCatalog
                .Obtenir(EmailTemplateKeys.AccountWelcome)
                .Rendre(
                    EmailValues.De(
                        (EmailVariables.NomMembre, user.FullName ?? string.Empty),
                        (EmailVariables.UrlCompte, _email.BuildUrl(CommunityUrl))),
                    EmailBranding.DepuisOptions(_email)),
            cancellationToken);
    }

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

    /// <summary>
    /// Envoie un courriel sans laisser un échec d'acheminement remettre en cause le compte.
    /// </summary>
    /// <param name="envoi">Envoi à tenter.</param>
    /// <param name="email">Destinataire, pour le journal.</param>
    /// <returns>Tâche complète, que le message soit parti ou non.</returns>
    /// <remarks>
    /// Le compte est déjà enregistré quand ce courriel part. Remonter l'erreur ferait échouer
    /// la requête, et le membre qui recommencerait sa saisie se heurterait à un doublon
    /// d'adresse. L'échec est journalisé ; l'adresse reste confirmable depuis un nouveau
    /// envoi.
    /// </remarks>
    private async Task TrySendAsync(Func<Task> envoi, string email)
    {
        try
        {
            await envoi();
        }
        catch (EmailDeliveryException)
        {
            LogCourrielCompteEchoue(logger, email);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Courriel de compte non remis à {Email} : le compte reste utilisable")]
    private static partial void LogCourrielCompteEchoue(ILogger logger, string email);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Courriel de confirmation non composé pour {Email} : aucun serveur de courriel n'est configuré")]
    private static partial void LogCourrielNonConfigure(ILogger logger, string email);
}

/// <summary>
/// Résultat d'un changement de mot de passe demandé par lien.
/// </summary>
/// <param name="IsSuccess">Vrai si le mot de passe a été remplacé.</param>
/// <param name="User">Compte concerné.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record PasswordResetResult(bool IsSuccess, ApplicationUser? User, string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte dont le mot de passe a été remplacé.</param>
    /// <returns>Résultat réussi.</returns>
    public static PasswordResetResult Success(ApplicationUser user) => new(true, user, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static PasswordResetResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Résultat d'une preuve de propriété d'un compte.
/// </summary>
/// <param name="IsSuccess">Vrai si le rattachement a eu lieu.</param>
/// <param name="User">Compte concerné.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record PasswordProofResult(bool IsSuccess, ApplicationUser? User, string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte rattaché.</param>
    /// <returns>Résultat réussi.</returns>
    public static PasswordProofResult Success(ApplicationUser user) => new(true, user, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static PasswordProofResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Résultat de la création d'un compte depuis un fournisseur externe.
/// </summary>
/// <param name="IsSuccess">Vrai si le compte a été créé.</param>
/// <param name="User">Compte créé.</param>
/// <param name="EmailVerified">Vrai si l'adresse a été certifiée par le fournisseur.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record ExternalAccountResult(
    bool IsSuccess,
    ApplicationUser? User,
    bool EmailVerified,
    string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte créé.</param>
    /// <param name="emailVerified">Vrai si l'adresse a été certifiée.</param>
    /// <returns>Résultat réussi.</returns>
    public static ExternalAccountResult Success(ApplicationUser user, bool emailVerified) =>
        new(true, user, emailVerified, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static ExternalAccountResult Failure(string error) => new(false, null, false, error);
}
