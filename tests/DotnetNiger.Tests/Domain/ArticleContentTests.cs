using DotnetNiger.Domain.Entities.Content;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie la préparation du corps rédigé dans l'éditeur : les schémas d'adresse
/// exécutables doivent être neutralisés, sans toucher au balisage légitime.
/// </summary>
public sealed class ArticleContentTests
{
    [Theory]
    [InlineData("<a href=\"javascript:alert(1)\">clic</a>", "href=\"#\"")]
    [InlineData("<a href=\"vbscript:msgbox(1)\">clic</a>", "href=\"#\"")]
    [InlineData("<a href=\"data:text/html,<script>x</script>\">clic</a>", "href=\"#\"")]
    [InlineData("<img src=\"file:///etc/passwd\">", "src=\"#\"")]
    [InlineData("<a href=\"JavaScript:alert(1)\">clic</a>", "href=\"#\"")]
    public void DepuisEditeur_NeutralisesDangerousScheme(string html, string attendu)
    {
        var resultat = ArticleContent.DepuisEditeur(html);

        Assert.Contains(attendu, resultat, StringComparison.Ordinal);
    }

    [Fact]
    public void DepuisEditeur_KeepsSafeMarkup()
    {
        const string html = "<p>Texte <strong>gras</strong> et <a href=\"https://exemple.test\">lien</a></p>";

        Assert.Equal(html, ArticleContent.DepuisEditeur(html));
    }

    [Fact]
    public void DepuisEditeur_KeepsSchemeInPlainText()
    {
        // Le mot « javascript » hors d'un attribut href/src n'est pas une adresse exécutable.
        const string html = "<p>Le mot javascript: est ici du texte.</p>";

        Assert.Equal(html, ArticleContent.DepuisEditeur(html));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DepuisEditeur_Throws_WhenEmpty(string? html)
    {
        Assert.ThrowsAny<ArgumentException>(() => ArticleContent.DepuisEditeur(html!));
    }

    [Fact]
    public void VersTexte_StripsTagsAndCollapsesSpaces()
    {
        var texte = ArticleContent.VersTexte("<p>Bonjour</p>\n\n<p>le    monde</p>");

        Assert.Equal("Bonjour le monde", texte);
    }

    [Fact]
    public void VersTexte_RemovesEntities()
    {
        var texte = ArticleContent.VersTexte("<p>Pain &amp; chocolat</p>");

        Assert.DoesNotContain("&amp;", texte, StringComparison.Ordinal);
        Assert.Contains("Pain", texte, StringComparison.Ordinal);
        Assert.Contains("chocolat", texte, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void VersTexte_Throws_WhenEmpty(string? html)
    {
        Assert.ThrowsAny<ArgumentException>(() => ArticleContent.VersTexte(html!));
    }
}
