namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Choisit l'icône d'un lien d'après l'adresse qu'il pointe.
/// </summary>
/// <remarks>
/// Les liens d'une fiche sont saisis par le membre : il y met son portfolio, son blog, ce
/// qu'il veut. On ne peut donc pas présumer qu'il n'y aura que trois réseaux. Cette classe
/// reconnaît ceux que le jeu d'icônes sait dessiner — GitHub et LinkedIn — et donne à tout
/// le reste l'icône d'un site, qui convient aussi bien à un portfolio qu'à un blog.
/// <para>
/// Le nom retourné est toujours l'un de ceux que <c>DnIcon</c> déclare : le composant ne
/// dessine rien d'inconnu, et une adresse non reconnue ne disparaîtrait pas.
/// </para>
/// </remarks>
internal static class IconesDeLien
{
    /// <summary>Icône d'un lien vers un site personnel : portfolio, blog, page d'accueil.</summary>
    public const string Site = "globe";

    /// <summary>
    /// Détermine l'icône correspondant à une adresse.
    /// </summary>
    /// <param name="url">Adresse du lien.</param>
    /// <returns>Le nom de l'icône.</returns>
    public static string Pour(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var adresse))
        {
            return Site;
        }

        var hote = adresse.Host.ToLowerInvariant();

        if (hote.Contains("github", StringComparison.Ordinal))
        {
            return "github";
        }

        if (hote.Contains("linkedin", StringComparison.Ordinal))
        {
            return "linkedin";
        }

        return Site;
    }
}
