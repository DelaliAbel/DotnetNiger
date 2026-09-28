using System.Net;
using System.Text.RegularExpressions;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Réduit une description rédigée en HTML au texte qu'un message peut afficher.
/// </summary>
/// <remarks>
/// Les ressources et les projets n'ont pas de phrase d'accroche : leur description est
/// enregistrée en HTML, à destination des pages du site. La lettre, elle, part en texte
/// brut et en HTML qu'un client de messagerie doit pouvoir rendre : y recopier une balise
/// telle quelle afficherait du code.
/// le retrait des balises est volontairement naïf. Le contenu vient de l'administration du
/// site, et une description n'a pas vocation à contenir de script ; un analyseur complet
/// serait ici une dépendance de plus pour un risque qui n'existe pas dans ce périmètre.
/// </remarks>
public static partial class TexteLisible
{
    /// <summary>
    /// Nombre de caractères au-delà duquel le texte est coupé.
    /// </summary>
    private const int LongueurMax = 240;

    /// <summary>
    /// Rend un texte brut à partir d'une description en HTML.
    /// </summary>
    /// <param name="html">Description enregistrée en HTML.</param>
    /// <returns>Texte brut, sur une seule ligne, d'une longueur bornée.</returns>
    public static string De(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var sansBalise = Balise().Replace(html, " ");
        var texte = WebUtility.HtmlDecode(sansBalise);
        var compact = Espaces().Replace(texte, " ").Trim();

        return compact.Length <= LongueurMax
            ? compact
            : string.Concat(compact.AsSpan(0, LongueurMax).TrimEnd(), "…");
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Balise();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espaces();
}
