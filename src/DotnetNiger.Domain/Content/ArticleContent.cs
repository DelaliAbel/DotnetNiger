using System.Text.RegularExpressions;
using Markdig;

namespace DotnetNiger.Domain.Content;

/// <summary>
/// Convertit le corps d'un article, saisi en Markdown, en HTML publiable.
/// </summary>
/// <remarks>
/// Le corps était produit par un éditeur WYSIWYG et stocké en HTML tel quel, ce qui
/// imposait de l'assainir à chaque lecture : une seule omission dans la liste blanche
/// suffisait à exécuter du script dans le navigateur d'un visiteur. On inverse le
/// rapport : le membre saisit du Markdown, et le HTML est fabriqué par le convertisseur,
/// qui ne laisse passer aucun balisage.
/// <para>
/// <c>DisableHtml</c> échappe les balises brutes — <c>&lt;script&gt;</c> et
/// <c>onerror</c> deviennent du texte. Il laisse en revanche passer un lien dont le
/// schéma est <c>javascript:</c> : le piège est dans l'adresse, pas dans la balise. Un
/// second passage retire donc ces schémas. Un membre peut publier un lien vers un site
/// construit pour l'arnaquer ; il ne peut pas faire exécuter du code par un visiteur.
/// </para>
/// </remarks>
public static class ArticleContent
{
    /// <summary>
    /// Convertit un corps Markdown en HTML sûr.
    /// </summary>
    /// <param name="markdown">Corps saisi par le membre.</param>
    /// <returns>HTML publiable, sans balisage ni schéma d'URL contrôlé par l'auteur.</returns>
    public static string VersHtml(string markdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markdown);

        var html = Markdown.ToHtml(markdown, Pipeline);

        return SchemasDangereux.Replace(html, match => $"{match.Groups[1].Value}=\"#\"");
    }

    /// <summary>
    /// Retire le corps d'un HTML pour n'en garder que le texte.
    /// </summary>
    /// <param name="html">HTML dont on ne veut garder que le texte.</param>
    /// <returns>Texte brut, balises retirées et espaces normalisés.</returns>
    /// <remarks>
    /// Sert au chapo d partage et à l'aperçu d'administration, où le HTML n'a pas sa
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
    /// Convertisseur sans HTML brut : le balisage écrit par le membre est échappé,
    /// jamais interprété.
    /// </summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .Build();

    /// <summary>
    /// Schémas d'URL qui exécutent du code dans la page du visiteur. Filtrés après
    /// conversion, car <c>DisableHtml</c> ne les touche pas : ils sont légitimes du
    /// point de vue du convertisseur, dangereux du point de vue du rendu.
    /// </summary>
    private static readonly Regex SchemasDangereux = new(
        @"(href|src)\s*=\s*""\s*(?:javascript|vbscript|data|file)\s*:[^""]*""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex Balise = new("<[^>]+>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly Regex Entite = new("&[a-zA-Z#0-9]+;", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly Regex EspacesMultiples = new(@"\s{2,}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
}
