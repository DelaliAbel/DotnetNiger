namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Noms de schémas d'authentification et adresses techniques associées.
/// </summary>
public static class ExternalSchemes
{
    /// <summary>
    /// Schéma de connexion Google.
    /// </summary>
    public const string Google = "Google";

    /// <summary>
    /// Schéma de connexion avec un compte Microsoft.
    /// </summary>
    public const string Microsoft = "Microsoft";

    /// <summary>
    /// Schéma de connexion GitHub.
    /// </summary>
    public const string GitHub = "GitHub";

    /// <summary>
    /// Schéma de connexion Microsoft pour un locataire professionnel précis. Sert quand
    /// l'organisation n'autorise pas les comptes personnels.
    /// </summary>
    public const string MicrosoftOrganisational = "MicrosoftOrganisation";

    /// <summary>
    /// Renvoie le nom de schéma utilisé pour l'affichage d'un fournisseur.
    /// </summary>
    /// <param name="provider">Nom de configuration du fournisseur.</param>
    /// <returns>Nom de schéma.</returns>
    public static string ForProvider(string provider) => provider.ToUpperInvariant() switch
    {
        "GOOGLE" => Google,
        "MICROSOFT" => Microsoft,
        "GITHUB" => GitHub,
        _ => provider,
    };

    /// <summary>
    /// Renvoie le nom de schéma effectif d'un fournisseur, compte tenu de son locataire.
    /// </summary>
    /// <param name="provider">Nom de configuration du fournisseur.</param>
    /// <param name="tenant">Locataire Microsoft configuré.</param>
    /// <returns>Nom de schéma enregistré.</returns>
    /// <remarks>
    /// Un compte Microsoft professionnel est enregistré sous un autre schéma que le compte
    /// personnel. Interroger le fournisseur par son seul nom enverrait le défi vers un schéma
    /// absent, et l'erreur « aucun fournisseur ne correspond » serait inexplicable.
    /// </remarks>
    public static string ForProvider(string provider, string? tenant)
    {
        if (provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(tenant)
            && !tenant.Trim().Equals("common", StringComparison.OrdinalIgnoreCase))
        {
            return MicrosoftOrganisational;
        }

        return ForProvider(provider);
    }
}
