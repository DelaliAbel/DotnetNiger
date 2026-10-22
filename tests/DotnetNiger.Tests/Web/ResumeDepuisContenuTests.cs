using DotnetNiger.Web.Services.Ui;

namespace DotnetNiger.Tests.Web;

/// <summary>
/// Vérifie la fabrication du chapô : il alimente les listes et les aperçus de partage, et
/// doit rester du texte lisible, jamais des balises ni des entités.
/// </summary>
public sealed class ResumeDepuisContenuTests
{
    [Fact]
    public void De_RetireLesBalises()
    {
        var resume = ResumeDepuisContenu.De("<p>Bonjour le <strong>monde</strong></p>");

        Assert.Equal("Bonjour le monde", resume);
    }

    [Fact]
    public void De_SepareLesBlocsParUnEspace()
    {
        var resume = ResumeDepuisContenu.De("<p>Premier</p><p>Second</p>");

        Assert.Equal("Premier Second", resume);
    }

    [Fact]
    public void De_DecodeLesEntites()
    {
        var resume = ResumeDepuisContenu.De("<p>Pain &amp; chocolat</p>");

        Assert.Contains("&", resume, StringComparison.Ordinal);
        Assert.DoesNotContain("&amp;", resume, StringComparison.Ordinal);
    }

    [Fact]
    public void De_NormaliseLesEspaces()
    {
        var resume = ResumeDepuisContenu.De("<p>trop     d'espaces</p>");

        Assert.Equal("trop d'espaces", resume);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void De_RetourneVide_QuandContenuAbsent(string? contenu)
    {
        Assert.Equal(string.Empty, ResumeDepuisContenu.De(contenu));
    }

    [Fact]
    public void De_NeCoupePas_QuandLeTexteEstCourt()
    {
        const string texte = "Un texte court.";

        Assert.Equal(texte, ResumeDepuisContenu.De(texte, 220));
    }

    [Fact]
    public void De_CoupeSurUneFrontiereDeMot_AvecPointsDeSuspension()
    {
        var phrase = string.Join(' ', Enumerable.Repeat("mot", 100));

        var resume = ResumeDepuisContenu.De(phrase, 50);

        Assert.EndsWith("…", resume, StringComparison.Ordinal);
        Assert.DoesNotContain("…", resume[..^1], StringComparison.Ordinal);
        Assert.True(resume.Length <= 51, $"Longueur obtenue : {resume.Length}");
        // Aucun mot tronqué : le résultat ne se termine pas au milieu d'un « mot ».
        Assert.EndsWith("mot", resume[..^1], StringComparison.Ordinal);
    }

    [Fact]
    public void De_CoupeFranchement_QuandAucunEspaceAvantLaLimite()
    {
        var sansEspace = new string('a', 100);

        var resume = ResumeDepuisContenu.De(sansEspace, 40);

        Assert.Equal(new string('a', 40) + "…", resume);
    }

    [Fact]
    public void De_RespecteLaLongueurPersonnalisee()
    {
        var resume = ResumeDepuisContenu.De("abcdefghij", 5);

        Assert.Equal("abcde…", resume);
    }
}
