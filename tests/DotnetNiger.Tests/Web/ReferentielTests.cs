using DotnetNiger.Web.Services.Ui;

namespace DotnetNiger.Tests.Web;

/// <summary>
/// Vérifie le référentiel partagé : les filtres d'URL et le menu de la rédaction doivent
/// désigner la même rubrique, sinon un lien produit par l'administration ne filtre rien.
/// </summary>
public sealed class ReferentielTests
{
    public static TheoryData<IReadOnlyList<Referentiel.Rubrique>> ToutesLesListes => new()
    {
        Referentiel.RubriquesArticle,
        Referentiel.CategoriesRessource,
        Referentiel.TypesEvenement,
    };

    [Theory]
    [MemberData(nameof(ToutesLesListes))]
    public void AucuneListeNEstVide(IReadOnlyList<Referentiel.Rubrique> liste)
    {
        Assert.NotEmpty(liste);
    }

    [Theory]
    [MemberData(nameof(ToutesLesListes))]
    public void LesSlugsSontUniquesDansChaqueListe(IReadOnlyList<Referentiel.Rubrique> liste)
    {
        Assert.Equal(liste.Count, liste.Select(r => r.Slug).Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [MemberData(nameof(ToutesLesListes))]
    public void LesNomsSontUniquesDansChaqueListe(IReadOnlyList<Referentiel.Rubrique> liste)
    {
        Assert.Equal(liste.Count, liste.Select(r => r.Nom).Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [MemberData(nameof(ToutesLesListes))]
    public void ChaqueRubriqueADesChampsRenseignes(IReadOnlyList<Referentiel.Rubrique> liste)
    {
        Assert.All(liste, rubrique =>
        {
            Assert.False(string.IsNullOrWhiteSpace(rubrique.Slug));
            Assert.False(string.IsNullOrWhiteSpace(rubrique.Nom));
            Assert.False(string.IsNullOrWhiteSpace(rubrique.Couleur));
        });
    }

    /// <summary>Variantes de badge que la feuille de style sait dessiner.</summary>
    private static readonly string[] CouleursAdmises = ["violet", "vert", "orange", "ink"];

    [Theory]
    [MemberData(nameof(ToutesLesListes))]
    public void ChaqueCouleurEstUneVarianteDeBadgeConnue(IReadOnlyList<Referentiel.Rubrique> liste)
    {
        // Ces quatre variantes sont les seules que la feuille de style dessine.
        Assert.All(liste, rubrique => Assert.Contains(rubrique.Couleur, CouleursAdmises));
    }

    [Fact]
    public void RubriqueArticleParSlug_RetrouveUneRubrique()
    {
        var rubrique = Referentiel.RubriqueArticleParSlug("blazor");

        Assert.NotNull(rubrique);
        Assert.Equal("Blazor", rubrique!.Nom);
    }

    [Fact]
    public void RubriqueArticleParNom_EstLInverseDeLaListe()
    {
        var rubrique = Referentiel.RubriqueArticleParNom("C#");

        Assert.NotNull(rubrique);
        Assert.Equal("csharp", rubrique!.Slug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("inexistant")]
    public void LesRecherchesParSlugRetournentNull_QuandAbsent(string? slug)
    {
        Assert.Null(Referentiel.RubriqueArticleParSlug(slug));
        Assert.Null(Referentiel.CategorieRessourceParSlug(slug));
        Assert.Null(Referentiel.TypeEvenementParSlug(slug));
    }

    [Fact]
    public void RubriqueArticleParNom_RetourneNull_QuandAbsent()
    {
        Assert.Null(Referentiel.RubriqueArticleParNom("Rubrique qui n'existe pas"));
    }

    [Fact]
    public void ChaqueSlugDeRubriqueArticleEstRetrouveParSaCle()
    {
        Assert.All(Referentiel.RubriquesArticle, rubrique =>
            Assert.Same(rubrique, Referentiel.RubriqueArticleParSlug(rubrique.Slug)));
    }

    [Fact]
    public void ChaqueNomDeRubriqueArticleEstRetrouveParSonLibelle()
    {
        Assert.All(Referentiel.RubriquesArticle, rubrique =>
            Assert.Same(rubrique, Referentiel.RubriqueArticleParNom(rubrique.Nom)));
    }
}
