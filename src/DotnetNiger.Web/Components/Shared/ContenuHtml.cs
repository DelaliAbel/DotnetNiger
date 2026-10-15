using Ganss.Xss;

namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Corps d'un contenu, rendu comme le HTML qu'il est, assaini avant affichage.
/// </summary>
/// <remarks>
/// <para>
/// L'éditeur TinyMCE produit du HTML, stocké tel quel (jusqu'à
/// <c>FieldLimits.Content</c>). C'est ce composant, joué au moment du rendu, qui
/// garantit la neutralité du corps : il ne garde qu'une liste blanche de balises et
/// d'attributs, retire les gestionnaires d'événements et les schémas d'adresse
/// exécutables. Un contenu collé depuis une page piégée, même signé d'un auteur de
/// confiance, ne peut donc pas exécuter de code chez le visiteur.
/// </para>
/// <para>
/// Le retrait se fait à la lecture plutôt qu'à l'écriture : on peut durcir la règle
/// sans migrer les contenus déjà en base, et un changement de moteur de rendu n'ouvre
/// plus la question.
/// </para>
/// <para>
/// Le HTML restant est rendu tel quel : c'est bien du HTML, et c'est ce que la page doit
/// afficher.
/// </para>
/// </remarks>
public sealed class ContenuHtml
{
    /// <summary>
    /// Corps affiché, après assainissement.
    /// </summary>
    /// <param name="html">Corps stocké en base.</param>
    /// <returns>Le HTML à afficher, restreint à la liste blanche.</returns>
    public static string Nettoyer(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        Generateur ??= CreerSanitiseur();
        return Generateur.Sanitize(html);
    }

    /// <summary>
    /// Sanitiseur réutilisé, instancié paresseusement pour ne régler sa liste blanche
    /// qu'une fois.
    /// </summary>
    private static HtmlSanitizer? Generateur;

    private static HtmlSanitizer CreerSanitiseur()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");
        sanitizer.AllowedTags.Add("hr");
        sanitizer.AllowedTags.Add("div");
        sanitizer.AllowedTags.Add("span");
        sanitizer.AllowedTags.Add("h1");
        sanitizer.AllowedTags.Add("h2");
        sanitizer.AllowedTags.Add("h3");
        sanitizer.AllowedTags.Add("h4");
        sanitizer.AllowedTags.Add("h5");
        sanitizer.AllowedTags.Add("h6");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("em");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("s");
        sanitizer.AllowedTags.Add("sub");
        sanitizer.AllowedTags.Add("sup");
        sanitizer.AllowedTags.Add("small");
        sanitizer.AllowedTags.Add("mark");
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedTags.Add("img");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("ol");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedTags.Add("blockquote");
        sanitizer.AllowedTags.Add("dl");
        sanitizer.AllowedTags.Add("dt");
        sanitizer.AllowedTags.Add("dd");
        sanitizer.AllowedTags.Add("pre");
        sanitizer.AllowedTags.Add("code");
        sanitizer.AllowedTags.Add("kbd");
        sanitizer.AllowedTags.Add("figure");
        sanitizer.AllowedTags.Add("figcaption");
        sanitizer.AllowedTags.Add("table");
        sanitizer.AllowedTags.Add("thead");
        sanitizer.AllowedTags.Add("tbody");
        sanitizer.AllowedTags.Add("tfoot");
        sanitizer.AllowedTags.Add("tr");
        sanitizer.AllowedTags.Add("th");
        sanitizer.AllowedTags.Add("td");
        sanitizer.AllowedTags.Add("caption");
        sanitizer.AllowedTags.Add("col");
        sanitizer.AllowedTags.Add("colgroup");

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("title");
        sanitizer.AllowedAttributes.Add("target");
        sanitizer.AllowedAttributes.Add("rel");
        sanitizer.AllowedAttributes.Add("width");
        sanitizer.AllowedAttributes.Add("height");
        sanitizer.AllowedAttributes.Add("colspan");
        sanitizer.AllowedAttributes.Add("rowspan");
        sanitizer.AllowedAttributes.Add("scope");
        sanitizer.AllowedAttributes.Add("headers");

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("ftp");

        // Sans propriété CSS autorisée, l'attribut style n'a aucune assise sur laquelle
        // survivre : il est retiré avec l'ensemble de la mise en forme qu'il portait.
        sanitizer.AllowedCssProperties.Clear();

        return sanitizer;
    }
}