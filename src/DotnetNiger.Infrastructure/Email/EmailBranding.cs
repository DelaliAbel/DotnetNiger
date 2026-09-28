using System.Collections.Frozen;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Éléments d'identité utilisés dans tous les courriels.
/// </summary>
/// <param name="SiteUrl">Adresse publique du site, terminée ou non par une barre.</param>
/// <param name="ContactAddress">Adresse de contact, affichée en pied de message si elle est renseignée.</param>
public sealed record EmailBranding(string SiteUrl, string? ContactAddress)
{
    /// <summary>Nom du site, tel qu'il apparaît en bandeau des messages.</summary>
    public const string SiteName = "dotnetNiger";

    /// <summary>Accroche affichée sous le nom du site.</summary>
    public const string Tagline = "La communauté .NET du Niger";

    /// <summary>
    /// Adresse du logo affichée en bandeau.
    /// </summary>
    public string LogoUrl => $"{SiteUrl.TrimEnd('/')}/favicon.png";

    /// <summary>
    /// Construit une adresse absolue à partir d'un chemin de page du site.
    /// </summary>
    /// <param name="cheminRelatif">Chemin commençant par <c>/</c>.</param>
    /// <returns>Adresse absolue.</returns>
    public string Page(string cheminRelatif) =>
        $"{SiteUrl.TrimEnd('/')}/{cheminRelatif.TrimStart('/')}";

    /// <summary>
    /// Jeu d'identité fictif, pour l'aperçu des modèles et les tests.
    /// </summary>
    public static EmailBranding Fictif { get; } = new(
        "https://dotnetniger.test",
        "contact@dotnetniger.test");

    /// <summary>
    /// Construit la marque à partir de la configuration du serveur d'envoi.
    /// </summary>
    /// <param name="options">Options configurées.</param>
    /// <returns>La marque correspondante.</returns>
    public static EmailBranding DepuisOptions(EmailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new EmailBranding(
            options.IsConfigured ? options.SiteUrl : Fictif.SiteUrl,
            string.IsNullOrWhiteSpace(options.ContactAddress) ? null : options.ContactAddress);
    }

    /// <summary>
    /// Liens de pied de message.
    /// </summary>
    /// <remarks>
    /// La liste est volontairement courte : seuls l'accueil et la connexion existent déjà
    /// comme pages. Ajouter les pages communauté, événements, ressources et contact
    /// enrichira ce pied au moment où elles existeront, plutôt que d'envoyer des liens
    /// morts dans la boîte de chaque membre abonné.
    /// </remarks>
    public FrozenDictionary<string, string> LiensDuPied { get; } =
        FrozenDictionary.ToFrozenDictionary(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Accueil"] = "/",
                ["Se connecter"] = "/compte/connexion",
            });
}
