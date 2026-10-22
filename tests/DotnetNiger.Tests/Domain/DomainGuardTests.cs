using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie les invariants partagés : c'est le socle que chaque entité réutilise, donc la
/// couche où une régression coûterait le plus cher.
/// </summary>
public sealed class DomainGuardTests
{
    [Fact]
    public void AgainstNull_ReturnsValue_WhenPresent()
    {
        var value = new object();

        Assert.Same(value, DomainGuard.AgainstNull(value, "reference"));
    }

    [Fact]
    public void AgainstNull_Throws_WhenMissing()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstNull<object>(null, "reference"));

        Assert.Equal("required_reference_missing", erreur.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_Throws_WhenEmpty(string? valeur)
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstNullOrWhiteSpace(valeur, "titre"));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_TrimsValue()
    {
        Assert.Equal("dotnet", DomainGuard.AgainstNullOrWhiteSpace("  dotnet  ", "titre"));
    }

    [Fact]
    public void AgainstTooLong_ReturnsValue_AtExactLimit()
    {
        var valeur = new string('a', 10);

        Assert.Equal(valeur, DomainGuard.AgainstTooLong(valeur, 10, "texte"));
    }

    [Fact]
    public void AgainstTooLong_Throws_WhenOverLimit()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstTooLong(new string('a', 11), 10, "texte"));

        Assert.Equal("field_too_long", erreur.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OptionalAgainstTooLong_ReturnsNull_WhenEmpty(string? valeur)
    {
        Assert.Null(DomainGuard.OptionalAgainstTooLong(valeur, 10, "texte"));
    }

    [Fact]
    public void OptionalAgainstTooLong_TrimsValue()
    {
        Assert.Equal("court", DomainGuard.OptionalAgainstTooLong("  court  ", 10, "texte"));
    }

    [Fact]
    public void OptionalAgainstTooLong_StillThrows_WhenTooLong()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.OptionalAgainstTooLong(new string('a', 11), 10, "texte"));

        Assert.Equal("field_too_long", erreur.Code);
    }

    [Fact]
    public void AgainstEmpty_ReturnsId_WhenSet()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, DomainGuard.AgainstEmpty(id, "auteur"));
    }

    [Fact]
    public void AgainstEmpty_Throws_WhenGuidEmpty()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstEmpty(Guid.Empty, "auteur"));

        Assert.Equal("field_required", erreur.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    public void AgainstNegative_ReturnsValue_WhenNotNegative(int valeur)
    {
        Assert.Equal(valeur, DomainGuard.AgainstNegative(valeur, "places"));
    }

    [Fact]
    public void AgainstNegative_Throws_WhenNegative()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstNegative(-1, "places"));

        Assert.Equal("field_negative", erreur.Code);
    }

    [Theory]
    [InlineData("http://exemple.test/page")]
    [InlineData("https://exemple.test/page?x=1")]
    public void AgainstHttpUrl_AcceptsHttpAndHttps(string url)
    {
        Assert.Equal(url, DomainGuard.AgainstHttpUrl(url, "lien"));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("/chemin/relatif")]
    [InlineData("ftp://exemple.test/fichier")]
    [InlineData("pas une url")]
    public void AgainstHttpUrl_RejectsAnythingElse(string url)
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstHttpUrl(url, "lien"));

        Assert.Equal("field_invalid_url", erreur.Code);
    }

    [Fact]
    public void AgainstHttpUrl_Throws_WhenEmpty()
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstHttpUrl("   ", "lien"));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void OptionalAgainstHttpUrl_ReturnsNull_WhenEmpty()
    {
        Assert.Null(DomainGuard.OptionalAgainstHttpUrl(null, "lien"));
        Assert.Null(DomainGuard.OptionalAgainstHttpUrl("  ", "lien"));
    }

    [Theory]
    [InlineData("https://cdn.exemple.test/image.png")]
    [InlineData("http://cdn.exemple.test/image.png")]
    [InlineData("/api/images/photo.webp")]
    public void AgainstImageSource_AcceptsAbsoluteUrlOrSitePath(string source)
    {
        Assert.Equal(source, DomainGuard.AgainstImageSource(source, "image"));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/svg+xml;base64,PHN2Zz4=")]
    [InlineData("images/relative.png")]
    [InlineData("/api/images/pas:de:deuxpoints")]
    public void AgainstImageSource_RejectsAnythingElse(string source)
    {
        var erreur = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstImageSource(source, "image"));

        Assert.Equal("field_invalid_url", erreur.Code);
    }

    [Fact]
    public void OptionalAgainstImageSource_ReturnsNull_WhenEmpty()
    {
        Assert.Null(DomainGuard.OptionalAgainstImageSource(null, "image"));
        Assert.Null(DomainGuard.OptionalAgainstImageSource(" ", "image"));
    }
}
