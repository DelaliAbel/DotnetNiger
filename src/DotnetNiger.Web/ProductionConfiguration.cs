using System.Globalization;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Services.Ui;

namespace DotnetNiger.Web;

/// <summary>
/// Vérifie qu'une configuration de production ne contient ni valeur manquante ni valeur de
/// développement restée en place.
/// </summary>
/// <remarks>
/// Un site déployé avec la configuration de développement ne lève aucune erreur : il démarre,
/// répond, et le défaut ne se révèle qu'au moment où quelqu'un tente de s'inscrire, de
/// réinitialiser un mot de passe ou d'ouvrir le back-office. Le courriel en particulier
/// s'inscrit dans le journal au lieu de partir, ce qui rend la panne invisible.
/// <para>
/// Cette classe ne corrige rien et n'invente aucune valeur : elle décrit ce qui empêche la
/// mise en service. C'est l'exploitation qui fournit les réglages, et l'application refuse
/// alors de démarrer plutôt que d'accepter une demi-configuration.
/// </para>
/// <para>
/// Les méthodes sont pures et séparées de la décision de lever l'exception, afin que les
/// règles soient vérifiables sans démarrer l'application.
/// </para>
/// </remarks>
public static class ProductionConfiguration
{
    /// <summary>
    /// Fragments d'une chaîne de connexion qui trahissent un poste de développement.
    /// </summary>
    private static readonly string[] ConnexionsDeDeveloppement =
    [
        "mssqllocaldb",
        "(localdb)",
        "Trusted_Connection=True",
        "Integrated Security",
    ];

    /// <summary>
    /// Décrit les réglages qui interdisent de démarrer en production.
    /// </summary>
    /// <param name="configuration">Configuration effective de l'application.</param>
    /// <returns>Réglages à corriger, formulés comme des consignes. Vide si la configuration est exploitable.</returns>
    public static IReadOnlyList<string> DescribeBlockingProblems(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var problemes = new List<string>(4);

        VerifierConnexion(configuration, problemes);
        VerifierCourriels(configuration, problemes);

        return problemes;
    }

    /// <summary>
    /// Décrit les réglages qui ne bloquent pas le démarrage mais demandent une décision.
    /// </summary>
    /// <param name="configuration">Configuration effective de l'application.</param>
    /// <returns>Points de vigilance. Vide si rien n'attire l'attention.</returns>
    public static IReadOnlyList<string> DescribeWarnings(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var points = new List<string>(3);

        if (configuration.GetSection(AdminBootstrapOptions.SectionName).Get<AdminBootstrapOptions>() is { IsConfigured: true })
        {
            points.Add(
                $"{AdminBootstrapOptions.SectionName} reste renseigné. Le compte administrateur existe déjà, "
                + "donc ces valeurs ne servent plus à rien : retirer l'adresse et le mot de passe "
                + "évite de laisser un mot de passe d'amorçage en clair sur le serveur.");
        }

        if (configuration.GetSection("AllowedHosts").Value == "*")
        {
            points.Add(
                "AllowedHosts vaut « * » : tout en-tête Host est accepté. Le restreindre au nom de "
                + "domaine dès que le domaine public est connu.");
        }

        return points;
    }

    private static void VerifierConnexion(IConfiguration configuration, List<string> problemes)
    {
        var connexion = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connexion))
        {
            problemes.Add(
                "ConnectionStrings:Default est absente. La base de données du site n'est pas "
                + "désignée : renseigner la chaîne fournie par l'hébergeur.");

            return;
        }

        foreach (var fragment in ConnexionsDeDeveloppement)
        {
            if (connexion.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                problemes.Add(
                    $"ConnectionStrings:Default désigne encore un poste de développement ({fragment}). "
                    + "Remplacer par la chaîne de l'hébergeur, avec identifiant et mot de passe SQL.");

                return;
            }
        }
    }

    private static void VerifierCourriels(IConfiguration configuration, List<string> problemes)
    {
        var section = configuration.GetSection("Email");
        var siteUrl = section["SiteUrl"];

        if (string.IsNullOrWhiteSpace(siteUrl))
        {
            problemes.Add(
                "Email:SiteUrl est absente. Les liens de confirmation d'inscription et de "
                + "réinitialisation de mot de passe seraient construits sur une adresse vide, donc "
                + "inutilisables. Renseigner l'adresse publique complète du site, par exemple "
                + "https://www.exemple.ne.");
        }
        else if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            problemes.Add(
                "Email:SiteUrl n'est pas une adresse absolue en https. Les liens envoyés aux "
                + "membres doivent pointer le site public et sécurisé.");
        }

        if (string.IsNullOrWhiteSpace(section["Host"]))
        {
            problemes.Add(
                "Email:Host est absent. Aucun message ne part : la confirmation d'inscription "
                + "échouerait pour tout nouveau membre. Renseigner le relais d'envoi et son port.");
        }

        if (string.IsNullOrWhiteSpace(section["FromAddress"]))
        {
            problemes.Add(
                "Email:FromAddress est absente. Sans expéditeur, l'envoi est refusé par le relais "
                + "ou classé comme courrier indésirable. Renseigner l'adresse du compte expéditeur.");
        }
    }
}
