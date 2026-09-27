namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Paramètres d'un fournisseur d'identité externe.
/// </summary>
public sealed class ExternalProviderOptions
{
    /// <summary>
    /// Identifiant client, fourni par le fournisseur.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Secret client. À fournir par un secret du serveur, jamais dans un fichier versionné.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Locataire Microsoft, par exemple <c>common</c> pour tous les comptes personnels
    /// et professionnels. Sans effet pour les autres fournisseurs.
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Vrai si l'identifiant et le secret sont tous deux présents.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret);
}

/// <summary>
/// Racine de la configuration des fournisseurs d'identité externe.
/// </summary>
public sealed class ExternalProvidersOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "ExternalProviders";

    /// <summary>
    /// Fournisseurs activés, avec leur nom lisible et la clé de configuration associée.
    /// </summary>
    public Dictionary<string, ExternalProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Fournisseurs prêts à l'emploi, dans l'ordre d'affichage.
    /// </summary>
    public static IReadOnlyDictionary<string, string> KnownProviders { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Google"] = "Google",
            ["Microsoft"] = "Microsoft",
            ["GitHub"] = "GitHub",
        };
}
