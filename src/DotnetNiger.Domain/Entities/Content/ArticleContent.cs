using System.Text.RegularExpressions;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Prépare le corps d'un contenu rédigé dans l'éditeur HTML de l'administration.
/// </summary>
/// <remarks>
/// L'éditeur produit du HTML (TinyMCE). Le corps est stocké tel quel — à la limite de
/// longueur fixée par <see cref="Common.FieldLimits.Content"/> — et sa neutralité est garantie
/// à l'affichage : <c>ContenuHtml.Nettoyer</c> retire les éléments d'exécution avant que
/// le navigateur ne les interprète.
/// <para>
/// Deux dangers restent maîtrisés ici, à l'écriture : les schémas d'adresse exécutables
/// (<c>javascript:</c>, <c>vbscript:</c>, <c>data:</c>, <c>file:</c>) portés par les
/// attributs <c>href</c> et <c>src</c>. Ils sont remplacés par une cible inerte. Le
/// balisage, lui, n'est plus échappé : l'administrateur écrit du HTML, et le retrait des
/// éléments interdits se fait à la lecture. tout un contenu collé depuis une page piégée
/// ne peut donc pas exécuter de code chez le visiteur.
/// </para>
/// </remarks>
public static class ArticleContent
{
    /// <summary>
    /// Préserve le corps produit par l'éditeur pour le stockage.
    /// </summary>
    /// <param name="html">Corps saisi dans l'éditeur.</param>
    /// <returns>Le HTML prêt à stocker, sans schéma d'adresse maîtrisé par l'auteur.</returns>
    public static string DepuisEditeur(string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);

        return SchemasDangereux.Replace(html, match => $"{match.Groups[1].Value}=\"#\"");
    }

    /// <summary>
    /// Retire le corps d'un HTML pour n'en garder que le texte.
    /// </summary>
    /// <param name="html">HTML dont on ne veut garder que le texte.</param>
    /// <returns>Texte brut, balises retirées et espaces normalisés.</returns>
    /// <remarks>
    /// Sert au chapo de partage et à l'aperçu d'administration, où le HTML n'a pas sa
    /// place dans une carte ou un paragraphe unique.
    /// </remarks>
    public static string VersTexte(string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);

        var sansBalise = Balise.Replace(html, " ");
        var sansEntite = Entite.Replace(sansBalise, " ");

        return EspacesMultiples.Replace(sansEntite, " ").Trim();
    }

    /// <summary>
    /// Schémas d'URL qui exécutent du code dans la page du visiteur. Le HTML de
    /// l'éditeur les accepte tels quels : le piège est dans l'adresse, pas dans la balise.
    /// Un contenu peut proposer un lien vers un site construit pour l'arnaquer ; il ne
    /// peut pas faire exécuter du code par le navigateur.
    /// </summary>
    private static readonly Regex SchemasDangereux = new(
        @"(href|src)\s*=\s*""\s*(?:javascript|vbscript|data|file)\s*:[^""]*""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex Balise = new("<[^>]+>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly Regex Entite = new("&[a-zA-Z#0-9]+;", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly Regex EspacesMultiples = new(@"\s{2,}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
}