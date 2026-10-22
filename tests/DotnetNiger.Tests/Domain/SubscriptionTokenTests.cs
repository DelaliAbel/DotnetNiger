using DotnetNiger.Domain.Common;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie les jetons d'abonnement : ils sont URL-safe, imprévisibles, et seule leur
/// empreinte doit être stable.
/// </summary>
public sealed class SubscriptionTokenTests
{
    [Fact]
    public void Creer_ReturnsDistinctTokenAndHash()
    {
        var (jeton, empreinte) = SubscriptionToken.Creer();

        Assert.NotEmpty(jeton);
        Assert.NotEmpty(empreinte);
        Assert.NotEqual(jeton, empreinte);
    }

    [Fact]
    public void Creer_EmpreinteMatchesHasher()
    {
        var (jeton, empreinte) = SubscriptionToken.Creer();

        Assert.Equal(SubscriptionToken.Hasher(jeton), empreinte);
    }

    [Fact]
    public void Creer_ProducesUrlSafeToken()
    {
        for (var i = 0; i < 200; i++)
        {
            var (jeton, _) = SubscriptionToken.Creer();

            Assert.DoesNotContain('+', jeton);
            Assert.DoesNotContain('/', jeton);
            Assert.DoesNotContain('=', jeton);
        }
    }

    [Fact]
    public void Creer_ProducesDifferentTokensEachTime()
    {
        var jetons = Enumerable.Range(0, 100).Select(_ => SubscriptionToken.Creer().Jeton).ToHashSet();

        Assert.Equal(100, jetons.Count);
    }

    [Fact]
    public void Hasher_IsDeterministicAndLowercaseHex()
    {
        var empreinte = SubscriptionToken.Hasher("valeur-stable");

        Assert.Equal(empreinte, SubscriptionToken.Hasher("valeur-stable"));
        Assert.Equal(empreinte.ToLowerInvariant(), empreinte);
        Assert.Equal(64, empreinte.Length);
    }

    [Fact]
    public void Hasher_DiffersForDifferentInput()
    {
        Assert.NotEqual(SubscriptionToken.Hasher("a"), SubscriptionToken.Hasher("b"));
    }

    [Fact]
    public void CreerCleDesabonnement_IsUrlSafeAndUnique()
    {
        var cles = Enumerable.Range(0, 50).Select(_ => SubscriptionToken.CreerCleDesabonnement()).ToList();

        Assert.All(cles, cle => Assert.DoesNotContain('+', cle));
        Assert.Equal(50, cles.Distinct().Count());
    }
}
