using System.Collections.Frozen;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Éléments d'identité utilisés dans tous les courriels.
/// </summary>
/// <param name="SiteUrl">Adresse publique du site, terminée ou non par une barre.</param>
/// <param name="ContactAddress">Adresse de contact, affichée en pied de message si elle est renseignée.</param>
/// <param name="Logo">
/// Adresse du logo déposé par l'administration, ou <see langword="null"/> pour reprendre
/// l'icône du site.
/// </param>
/// <remarks>
/// Le logo vient des réglages du site, et non du fichier de configuration : c'est une valeur
/// que l'administration change sans toucher au serveur, et un courriel qui porterait encore
/// l'ancienne icône après son remplacement serait fausse. Il est stocké sous forme
/// relative — <c>/api/images/…</c> — et rendu absolu ici : c'est ce qui permet de changer de
/// nom de domaine sans réécrire le réglage.
/// </remarks>
public sealed record EmailBranding(string SiteUrl, string? ContactAddress, string? Logo = null)
{
    /// <summary>Nom du site, tel qu'il apparaît en bandeau des messages.</summary>
    public const string SiteName = "DotnetNiger";

    /// <summary>Accroche affichée sous le nom du site.</summary>
    public const string Tagline = "La communauté .NET du Niger";

    /// <summary>
    /// Adresse absolue du logo affichée en bandeau. Un réglage relatif est rattaché à
    /// l'adresse du site ; une adresse déjà en <c>http</c> est reprise telle quelle.
    /// </summary>
    /// <remarks>
    /// Le test de préfixe est délibéré, et non <c>UriKind.Absolute</c> : sous Linux, un
    /// chemin comme <c>/api/images/12</c> <em>est</em> une URI absolue — de type fichier. La
    /// prendre pour une adresse web produirait un bandeau dont le logo ne se charge
    /// jamais, ce qui se voit à peine dans un client de messagerie mais se voit bien dans
    /// l'aperçu d'administration.
    /// </remarks>
    public string LogoUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Logo))
            {
                return $"{SiteUrl.TrimEnd('/')}/favicon.png";
            }

            var logo = Logo.Trim();

            return logo.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || logo.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? logo
                : $"{SiteUrl.TrimEnd('/')}/{logo.TrimStart('/')}";
        }
    }

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
        "contact@dotnetniger.test",
        null);

    /// <summary>
    /// Construit la marque à partir de la configuration du serveur d'envoi et des réglages
    /// du site.
    /// </summary>
    /// <param name="options">Options configurées.</param>
    /// <param name="logo">Logo déposé par l'administration, ou <see langword="null"/>.</param>
    /// <returns>La marque correspondante.</returns>
    public static EmailBranding DepuisOptions(EmailOptions options, string? logo = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new EmailBranding(
            options.IsConfigured ? options.SiteUrl : Fictif.SiteUrl,
            string.IsNullOrWhiteSpace(options.ContactAddress) ? null : options.ContactAddress,
            string.IsNullOrWhiteSpace(logo) ? null : logo);
    }

    /// <summary>
    /// Jeu de liens affichés en pied de message.
    /// </summary>
    /// <remarks>
    /// La liste est volontairement courte : seuls l'accueil, la connexion, les événements et
    /// la communauté existent comme pages. Y ajouter des routes qui n'existent pas enverrait
    /// des liens morts dans la boîte de chaque membre abonné.
    /// </remarks>
    public FrozenDictionary<string, string> LiensDuPied { get; } =
        FrozenDictionary.ToFrozenDictionary(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Accueil"] = "/",
                ["Événements"] = "/evenements",
                ["Communauté"] = "/communaute",
                ["Se connecter"] = "/compte/connexion",
            });
}
