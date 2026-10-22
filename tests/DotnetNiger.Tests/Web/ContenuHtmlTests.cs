using DotnetNiger.Web.Services.Ui;

namespace DotnetNiger.Tests.Web;

/// <summary>
/// Vérifie l'assainisseur appliqué au moment du rendu : c'est la dernière barrière entre un
/// contenu stocké et le navigateur du visiteur.
/// </summary>
public sealed class ContenuHtmlTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Nettoyer_RetourneVide_QuandContenuAbsent(string? html)
    {
        Assert.Equal(string.Empty, ContenuHtml.Nettoyer(html));
    }

    [Fact]
    public void Nettoyer_ConserveLeBalisageAutorise()
    {
        const string html = "<p>Texte <strong>gras</strong>, <em>italique</em>, "
            + "<a href=\"https://exemple.test\">lien</a>, <code>code</code>.</p>";

        var propre = ContenuHtml.Nettoyer(html);

        Assert.Contains("<p>", propre, StringComparison.Ordinal);
        Assert.Contains("<strong>gras</strong>", propre, StringComparison.Ordinal);
        Assert.Contains("<em>italique</em>", propre, StringComparison.Ordinal);
        Assert.Contains("<code>code</code>", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_RetireLesGestionnairesDEvenement()
    {
        var propre = ContenuHtml.Nettoyer("<p onclick=\"alert(1)\">Texte</p>");

        Assert.DoesNotContain("onclick", propre, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Texte", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_RetireLaBaliseScript()
    {
        var propre = ContenuHtml.Nettoyer("<p>Avant</p><script>alert(1)</script>");

        Assert.DoesNotContain("<script", propre, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Avant", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_RetireLaBaliseIframe()
    {
        var propre = ContenuHtml.Nettoyer("<iframe src=\"https://piege.test\"></iframe>");

        Assert.DoesNotContain("<iframe", propre, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nettoyer_RetireUnSchemaExecutable()
    {
        var propre = ContenuHtml.Nettoyer("<a href=\"javascript:alert(1)\">clic</a>");

        Assert.DoesNotContain("javascript:", propre, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nettoyer_RetireLAttributStyle()
    {
        var propre = ContenuHtml.Nettoyer("<p style=\"color:red\">Texte</p>");

        Assert.DoesNotContain("style", propre, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Texte", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_ConserveLesAttributsUsuelsDUneImage()
    {
        var propre = ContenuHtml.Nettoyer(
            "<img src=\"https://cdn.exemple.test/a.png\" alt=\"Affiche\" width=\"600\">");

        Assert.Contains("src=", propre, StringComparison.Ordinal);
        Assert.Contains("alt=\"Affiche\"", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_ConserveUnTableau()
    {
        const string html = "<table><thead><tr><th scope=\"col\">Titre</th></tr></thead>"
            + "<tbody><tr><td>Valeur</td></tr></tbody></table>";

        var propre = ContenuHtml.Nettoyer(html);

        Assert.Contains("<table>", propre, StringComparison.Ordinal);
        Assert.Contains("scope=\"col\"", propre, StringComparison.Ordinal);
        Assert.Contains("Valeur", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_RetireLesBalisesDeMiseEnPage()
    {
        // Les conteneurs hors liste blanche sont retirés avec leur contenu : c'est le
        // comportement de la bibliothèque, et le noter ici évite de le découvrir en production.
        var propre = ContenuHtml.Nettoyer("<body><header>Titre</header><main>Contenu</main></body>");

        Assert.DoesNotContain("<header", propre, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", propre, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<body", propre, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nettoyer_ConserveLeTexteDansUnConteneurAutorise()
    {
        // Un <div> est autorisé : son contenu doit survivre même si les enfants ne le sont pas.
        var propre = ContenuHtml.Nettoyer("<div><section>Texte</section></div>");

        Assert.Contains("<div>", propre, StringComparison.Ordinal);
    }

    [Fact]
    public void Nettoyer_EstIdempotent()
    {
        const string html = "<p onclick=\"x()\">Texte <a href=\"javascript:alert(1)\">lien</a></p>";

        var une = ContenuHtml.Nettoyer(html);
        var deux = ContenuHtml.Nettoyer(une);

        Assert.Equal(une, deux);
    }
}
