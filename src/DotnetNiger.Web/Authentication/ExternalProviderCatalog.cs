namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Fournisseur externe activable, prêt à être affiché.
/// </summary>
/// <param name="Key">Clé de configuration, utilisée dans les liens.</param>
/// <param name="Label">Nom affiché.</param>
public sealed record EnabledProvider(string Key, string Label);

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

            result.Add(new EnabledProvider(provider.ToLowerInvariant(), LabelFor(provider)));
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
}
