using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie la fabrique de slugs : elle alimente toutes les URL publiques, et un slug
/// instable casserait un lien déjà diffusé.
/// </summary>
public sealed class SlugFactoryTests
{
    [Fact]
    public void Create_LowercasesAndSeparatesWithDashes()
    {
        Assert.Equal("blazor-en-production", SlugFactory.Create("Blazor en production"));
    }

    [Fact]
    public void Create_RemovesDiacritics()
    {
        Assert.Equal("donnees-et-entities", SlugFactory.Create("Données et Entities"));
    }

    [Fact]
    public void Create_CollapsesAdjacentSeparators()
    {
        Assert.Equal("donnees-entities", SlugFactory.Create("Données & Entities"));
    }

    [Fact]
    public void Create_CollapsesPunctuationIntoASingleDash()
    {
        Assert.Equal("c-est-quoi-asp-net", SlugFactory.Create("C'est   quoi ??? ASP.NET"));
    }

    [Fact]
    public void Create_KeepsDigits()
    {
        Assert.Equal("net-10-en-2026", SlugFactory.Create(".NET 10 en 2026"));
    }

    [Fact]
    public void Create_TrimsLeadingAndTrailingSeparators()
    {
        Assert.Equal("blazor", SlugFactory.Create("---Blazor---"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!! ??? ...")]
    [InlineData("中文")]
    public void Create_Throws_WhenNoUsableCharacter(string? valeur)
    {
        var erreur = Assert.Throws<DomainException>(() => SlugFactory.Create(valeur!));

        Assert.Equal("invalid_slug", erreur.Code);
    }

    [Fact]
    public void Create_TruncatesToMaxLength()
    {
        var slug = SlugFactory.Create(new string('a', SlugFactory.MaxLength + 50));

        Assert.Equal(SlugFactory.MaxLength, slug.Length);
    }

    [Fact]
    public void Create_DoesNotEndWithADash_AfterTruncation()
    {
        // Un « a » isolé à la limite de coupe doit disparaître avec le tiret qui le précède.
        var valeur = new string('a', SlugFactory.MaxLength) + " b";

        var slug = SlugFactory.Create(valeur);

        Assert.EndsWith("a", slug, StringComparison.Ordinal);
        Assert.DoesNotContain("--", slug, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateWithSuffix_AppendsShortDiscriminator()
    {
        var id = Guid.Parse("0123456789abcdef0123456789abcdef");

        var slug = SlugFactory.CreateWithSuffix("Blazor", id);

        Assert.Equal("blazor-01234567", slug);
    }

    [Fact]
    public void CreateWithSuffix_StaysWithinMaxLength_ForLongBase()
    {
        var slug = SlugFactory.CreateWithSuffix(new string('a', 400), Guid.NewGuid());

        Assert.True(
            slug.Length <= SlugFactory.MaxLength,
            $"Longueur obtenue : {slug.Length}");
    }

    [Fact]
    public void CreateWithSuffix_ProducesDifferentSlugs_ForDifferentIds()
    {
        var premier = SlugFactory.CreateWithSuffix("meme-titre", Guid.NewGuid());
        var second = SlugFactory.CreateWithSuffix("meme-titre", Guid.NewGuid());

        Assert.NotEqual(premier, second);
    }
}
