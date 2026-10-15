using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Courriels du cycle de vie d'un compte : confirmation, bienvenue, et Resistance à l'échec d'envoi.
/// </summary>
public sealed partial class AccountOperations
{
    /// <summary>
    /// Envoie le courriel de confirmation et de bienvenue après une inscription locale.
    /// </summary>
    /// <param name="user">Compte créé.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche complète une fois les messages remis à l'expéditeur.</returns>
    /// <remarks>
    /// L'ordre des deux messages est celui demandé pour une inscription locale : d'abord la
    /// confirmation, puis la bienvenue une fois l'adresse validée. La bienvenue part de
    /// l'inscription parce que c'est le seul moment où l'on sait que l'adresse est la bonne ;
    /// elle est renvoyée à la confirmation.
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
                    await BrandeAsync()),
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
                    await BrandeAsync()),
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
                    await BrandeAsync()),
            cancellationToken);
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