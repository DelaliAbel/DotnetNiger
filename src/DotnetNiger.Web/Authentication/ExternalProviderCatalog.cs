namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Fournisseur externe activable, prêt à être affiché.
/// </summary>
/// <param name="Key">Clé de configuration, utilisée dans les liens.</param>
/// <param name="Label">Nom affiché.</param>
/// <param name="Icone">
/// Nom de l'icône dans la bibliothèque du site. Un bouton « continuer avec X » sans le logo
/// de X seCamoufle en lien ordinaire : l'internaute ne sait pas où il clique.
/// </param>
public sealed record EnabledProvider(string Key, string Label, string Icone);

/// <summary>
/// Liste des fournisseurs externes effectivement configurés.
/// </summary>
public static class ExternalProviderCatalog
{
    /// <summary>
    /// Fournisseurs activés dans la configuration courante.
    /// </summary>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Fournisseurs prêts à l'affichage, dans un ordre stable.</returns>
    /// <remarks>
    /// La page de connexion n'affiche que les fournisseurs réellement configurés. Un bouton
    /// vers un fournisseur sans identifiant mènerait à une erreur : mieux vaut ne pas le
    /// montrer.
    /// </remarks>
    public static IReadOnlyList<EnabledProvider> EnabledProviders(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var root = configuration
            .GetSection(ExternalProvidersOptions.SectionName)
            .Get<ExternalProvidersOptions>() ?? new ExternalProvidersOptions();

        var result = new List<EnabledProvider>();

        foreach (var (provider, settings) in root.Providers)
        {
            if (!settings.IsConfigured || !ExternalProvidersOptions.KnownProviders.ContainsKey(provider))
            {
                continue;
            }

            result.Add(new EnabledProvider(
                provider.ToLowerInvariant(),
                LabelFor(provider),
                IconeFor(provider)));
        }

        // L'ordre d'affichage ne doit pas dépendre de l'ordre des clés dans le fichier de
        // configuration, sans quoi deux environnements présenteraient des boutons différents.
        var ordre = ExternalProvidersOptions.KnownProviders.Keys
            .Select(cle => cle.ToLowerInvariant())
            .ToArray();

        return result
            .OrderBy(fournisseur => Array.IndexOf(ordre, fournisseur.Key))
            .ToList();
    }

    private static string LabelFor(string provider) => provider.ToUpperInvariant() switch
    {
        "GOOGLE" => "Google",
        "MICROSOFT" => "Microsoft",
        "GITHUB" => "GitHub",
        _ => provider,
    };

    /// <summary>
    /// Icône du logo, pour le bouton de connexion.
    /// </summary>
    /// <param name="provider">Clé de configuration du fournisseur.</param>
    /// <returns>Le nom de l'icône, ou <c>external-link</c> si le logo n'existe pas.</returns>
    /// <remarks>
    /// Microsoft n'a pas de logo dans la bibliothèque du site. Plutôt que d'ajouter un
    /// symbole approximatif, on garde l'icône générique : un logo Microsoft approximatif se
    /// lirait comme une faute, alors qu'une icône neutre reste honnête.
    /// </remarks>
    private static string IconeFor(string provider) => provider.ToUpperInvariant() switch
    {
        "GOOGLE" => "google",
        "GITHUB" => "github",
        _ => "external-link",
    };
}
