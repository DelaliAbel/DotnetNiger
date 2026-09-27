using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.UnitTests.Domain;

/// <summary>
/// Vérifie la fabrication des identifiants d'URL lisibles.
/// </summary>
public sealed class SlugFactoryTests
{
    [Theory]
    [InlineData("Nouvel article .NET 10", "nouvel-article-net-10")]
    [InlineData("  Espaces  autour  ", "espaces-autour")]
    [InlineData("Accents: éàüñ", "accents-eaun")]
    [InlineData("Symbôles &/%$#", "symboles")]
    [InlineData("Déjà vu", "deja-vu")]
    [InlineData("Migration vers .NET 10", "migration-vers-net-10")]
    public void Create_Transforme_le_titre_en_slug_lisible(string source, string attendu)
    {
        Assert.Equal(attendu, SlugFactory.Create(source));
    }

    [Theory]
    [InlineData("---")]
    [InlineData("???")]
    [InlineData("   ")]
    [InlineData("")]
    public void Create_Refuse_un_texte_sans_caractere_Utilisable(string source)
    {
        Assert.Throws<DomainException>(() => SlugFactory.Create(source));
    }

    [Fact]
    public void Create_Ne_Depasse_Pas_La_Limite_Declaree()
    {
        var slug = SlugFactory.Create(new string('a', 400));

        Assert.True(slug.Length <= SlugFactory.MaxLength);
    }

    [Fact]
    public void CreateWithSuffix_Distingue_Deux_Articles_de_Meme_Titre()
    {
        var premier = SlugFactory.CreateWithSuffix("Mon article", Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var second = SlugFactory.CreateWithSuffix("Mon article", Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.NotEqual(premier, second);
        Assert.StartsWith("mon-article-", premier, StringComparison.Ordinal);
        Assert.True(premier.Length <= SlugFactory.MaxLength);
    }
}
