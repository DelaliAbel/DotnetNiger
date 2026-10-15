using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Opérations de compte partagées par les pages et les points de terminaison.
/// </summary>
/// <remarks>
/// Ce fichier ne porte que ce que toutes les opérations ont en commun : l'adresse du site,
/// les adresses des pages du parcours, et le fait de savoir si un envoi de courriel est
/// possible. Les opérations elles-mêmes sont réparties dans les parties voisines.
/// </remarks>
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
}