using System.Security.Claims;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.IntegrationTests;

public class ExternalAccountLinkingTests
{
    [Theory]
    [InlineData("email_verified", "true")]
    [InlineData("email_verified", "True")]
    [InlineData("email_verified", "1")]
    [InlineData("verified", "true")]
    [InlineData("verified", "1")]
    public void IsEmailVerified_accepte_une_certification_explicite(string type, string valeur)
    {
        var claims = new[] { new Claim(type, valeur) };

        Assert.True(ExternalAccountLinking.IsEmailVerified(claims));
    }

    [Theory]
    [InlineData("email_verified", "false")]
    [InlineData("email_verified", "0")]
    [InlineData("verified", "false")]
    [InlineData("verified", "")]
    [InlineData("email_verified", "peut-être")]
    public void IsEmailVerified_refuse_une_certification_nulle(string type, string valeur)
    {
        var claims = new[] { new Claim(type, valeur) };

        Assert.False(ExternalAccountLinking.IsEmailVerified(claims));
    }

    [Fact]
    public void IsEmailVerified_refuse_quand_la_revendication_est_absente()
    {
        // Cas de GitHub : l'adresse est bien transmise, mais le fournisseur n'atteste pas
        // qu'elle est vérifiée. Le silence ne vaut pas certification.
        var claims = new[] { new Claim(ClaimTypes.Email, "membre@example.org") };

        Assert.False(ExternalAccountLinking.IsEmailVerified(claims));
    }

    [Fact]
    public void IsEmailVerified_refuse_une_collection_vide()
    {
        Assert.False(ExternalAccountLinking.IsEmailVerified([]));
    }

    [Fact]
    public void IsEmailVerified_accepte_une_revendication_majuscule()
    {
        var claims = new[] { new Claim("Email_Verified", "true") };

        Assert.True(ExternalAccountLinking.IsEmailVerified(claims));
    }

    [Fact]
    public void IsEmailVerified_refuse_une_collection_nulle()
    {
        Assert.Throws<ArgumentNullException>(
            () => ExternalAccountLinking.IsEmailVerified(null!));
    }
}
