using System.Net;
using System.Text.RegularExpressions;

namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Fabrique le chapô d'un contenu à partir de son texte.
/// </summary>
/// <remarks>
/// Le chapô n'est plus une ligne que l'administrateur saisit à côté du contenu : c'est le
/// début du texte lui-même. Une seule source, donc rien à resynchroniser — et le résumé
/// affiché dans les listes ne peut pas diverger de ce que l'article dit réellement.
/// <para>
/// La coupure se fait sur une frontière de mot : une coupure au milieu d'un mot donne un
/// texte qui n'existe nulle part ailleurs dans le site.
/// </para>
/// </remarks>
public static partial class ResumeDepuisContenu
{
    /// <summary>Longueur visée du chapô, hors points de suspension.</summary>
    private const int Longueur = 220;

    /// <summary>
    /// Extrait le chapô du début d'un contenu rédigé en HTML.
    /// </summary>
    /// <param name="contenuHtml">Corps du contenu, tel que l'éditeur l'a produit.</param>
    /// <param name="longueur">Longueur visée, 220 caractères par défaut.</param>
    /// <returns>Le chapô, terminé par des points de suspension s'il a été coupé.</returns>
    public static string De(string? contenuHtml, int longueur = Longueur)
    {
        var texte = EnTexte(contenuHtml);

        if (texte.Length <= longueur)
        {
            return texte;
        }

        // On coupe au dernier espace avant la longueur visée : le mot entier est conservé,
        // et les espaces multiples qu'il.trainait sont supprimés au passage.
        var coupe = texte.LastIndexOf(' ', longueur);

        if (coupe < longueur / 2)
        {
            coupe = longueur;
        }

        return texte[..coupe].TrimEnd() + "…";
    }

    /// <summary>
    /// Réduit un HTML à son texte lisible.
    /// </summary>
    /// <remarks>
    /// Les blocs sont séparés par un espace plutôt que accolés : sans cela, deux paragraphes
    /// se rejoignent par « motun ». Les entités HTML sont décodées, sinon le résumé se
    /// lirait « &amp; » dans les listes du site.
    /// </remarks>
    /// <param name="contenuHtml">Corps du contenu.</param>
    /// <returns>Le texte, espaces normalisés.</returns>
    private static string EnTexte(string? contenuHtml)
    {
        if (string.IsNullOrWhiteSpace(contenuHtml))
        {
            return string.Empty;
        }

        var sansBalises = Html.Replace(contenuHtml, " ");

        return Espaces.Replace(WebUtility.HtmlDecode(sansBalises), " ").Trim();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex Html { get; }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex Espaces { get; }
}