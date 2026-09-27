using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.UnitTests.Domain;

/// <summary>
/// Vérifie les garde-fous partagés par toutes les entités : ils traduisent une entrée
/// invalide en <see cref="DomainException"/> plutôt qu'en valeur corrompue en base.
/// </summary>
public sealed class DomainGuardTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_Refuse_les_valeurs_absentes(string? valeur)
    {
        var exception = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstNullOrWhiteSpace(valeur, "Titre"));

        Assert.Contains("Titre", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_Retourne_La_Valeur_Nettoyee()
    {
        Assert.Equal("Article", DomainGuard.AgainstNullOrWhiteSpace("  Article  ", "Titre"));
    }

    [Fact]
    public void AgainstTooLong_Refuse_Le_Depassement()
    {
        var exception = Assert.Throws<DomainException>(
            () => DomainGuard.AgainstTooLong(new string('x', 11), 10, "Titre"));

        Assert.Contains("10", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AgainstTooLong_Accepte_La_Limite_Exacte()
    {
        Assert.Equal(10, DomainGuard.AgainstTooLong(new string('x', 10), 10, "Titre").Length);
    }

    [Fact]
    public void OptionalAgainstTooLong_Traite_Le_Vide_Comme_Absent()
    {
        Assert.Null(DomainGuard.OptionalAgainstTooLong(null, 10, "Bio"));
        Assert.Null(DomainGuard.OptionalAgainstTooLong(string.Empty, 10, "Bio"));
        Assert.Null(DomainGuard.OptionalAgainstTooLong("   ", 10, "Bio"));
    }

    [Fact]
    public void OptionalAgainstTooLong_Accepte_Une_Valeur_Au_Dessous_De_La_Limite()
    {
        Assert.Equal("Dakar", DomainGuard.OptionalAgainstTooLong("  Dakar  ", 10, "Ville"));
    }

    [Fact]
    public void AgainstEmpty_Refuse_Guid_Vide()
    {
        Assert.Throws<DomainException>(() => DomainGuard.AgainstEmpty(Guid.Empty, "Auteur"));
    }

    [Fact]
    public void AgainstNegative_Refuse_Une_Valeur_Negative()
    {
        Assert.Throws<DomainException>(() => DomainGuard.AgainstNegative(-1, "Capacite"));
        Assert.Equal(0, DomainGuard.AgainstNegative(0, "Capacite"));
    }

    [Theory]
    [InlineData("https://dot.net")]
    [InlineData("http://dot.net")]
    public void AgainstHttpUrl_Accepte_Http_Et_Https(string url)
    {
        Assert.Equal(url, DomainGuard.AgainstHttpUrl(url, "Site"));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://dot.net")]
    [InlineData("dot.net")]
    [InlineData("data:text/html;base64,AAAA")]
    public void AgainstHttpUrl_Refuse_Tout_Le_Reste(string url)
    {
        Assert.Throws<DomainException>(() => DomainGuard.AgainstHttpUrl(url, "Site"));
    }

    [Fact]
    public void OptionalAgainstHttpUrl_Laisse_Passer_Null()
    {
        Assert.Null(DomainGuard.OptionalAgainstHttpUrl(null, "Site"));
    }
}
