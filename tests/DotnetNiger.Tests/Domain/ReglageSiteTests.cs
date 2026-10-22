using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie les réglages du site : la clé doit être connue, et la valeur ne peut pas porter
/// un schéma que le navigateur exécuterait dans un <c>href</c> ou un <c>src</c>.
/// </summary>
public sealed class ReglageSiteTests
{
    [Fact]
    public void Constructeur_AccepteUneCleConnue()
    {
        var reglage = new ReglageSite(ClesReglage.Logo, "/api/images/logo.svg");

        Assert.Equal(ClesReglage.Logo, reglage.Cle);
        Assert.Equal("/api/images/logo.svg", reglage.Valeur);
    }

    [Fact]
    public void Constructeur_TrimLaCle()
    {
        var reglage = new ReglageSite($"  {ClesReglage.Telephone}  ", "+225 07 00 00 00");

        Assert.Equal(ClesReglage.Telephone, reglage.Cle);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructeur_Throws_QuandCleVide(string? cle)
    {
        var erreur = Assert.Throws<DomainException>(() => new ReglageSite(cle!, "valeur"));

        Assert.Equal("setting_key_required", erreur.Code);
    }

    [Fact]
    public void Constructeur_Throws_QuandCleInconnue()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new ReglageSite("site.inconnu", "valeur"));

        Assert.Equal("setting_unknown_key", erreur.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructeur_UneValeurVideVautAbsence(string? valeur)
    {
        var reglage = new ReglageSite(ClesReglage.Courriel, valeur);

        Assert.Null(reglage.Valeur);
    }

    [Fact]
    public void Constructeur_Throws_QuandValeurTropLongue()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new ReglageSite(ClesReglage.Courriel, new string('a', 501)));

        Assert.Equal("setting_value_too_long", erreur.Code);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>x</script>")]
    [InlineData("file:///etc/passwd")]
    public void Constructeur_Throws_QuandSchemaInterdit(string valeur)
    {
        var erreur = Assert.Throws<DomainException>(
            () => new ReglageSite(ClesReglage.Logo, valeur));

        Assert.Equal("setting_scheme_forbidden", erreur.Code);
    }

    [Theory]
    [InlineData("https://exemple.test/page")]
    [InlineData("http://exemple.test/page")]
    [InlineData("mailto:contact@exemple.test")]
    [InlineData("tel:+2250700000000")]
    [InlineData("Abidjan, Côte d'Ivoire")]
    [InlineData("/api/images/logo.svg")]
    public void Constructeur_AccepteLesValeursAdmises(string valeur)
    {
        var reglage = new ReglageSite(ClesReglage.Courriel, valeur);

        Assert.Equal(valeur, reglage.Valeur);
    }

    [Fact]
    public void Constructeur_AccepteUnCheminDeSite_PourLeLogo()
    {
        // Le logo déposé par l'administration est renvoyé sous cette forme : la refuser
        // empêcherait d'enregistrer le logo que le site vient lui-même d'accepter.
        var reglage = new ReglageSite(ClesReglage.Logo, "/api/images/42");

        Assert.Equal("/api/images/42", reglage.Valeur);
    }

    [Fact]
    public void Definir_RemplaceLaValeur()
    {
        var reglage = new ReglageSite(ClesReglage.Whatsapp, "https://wa.me/1");

        reglage.Definir("https://wa.me/2");

        Assert.Equal("https://wa.me/2", reglage.Valeur);
    }

    [Fact]
    public void Definir_UneValeurVideRetireLeReglage()
    {
        var reglage = new ReglageSite(ClesReglage.Whatsapp, "https://wa.me/1");

        reglage.Definir("   ");

        Assert.Null(reglage.Valeur);
    }

    [Fact]
    public void Definir_Throws_QuandSchemaInterdit()
    {
        var reglage = new ReglageSite(ClesReglage.Github, "https://github.com/dotnetniger");

        var erreur = Assert.Throws<DomainException>(
            () => reglage.Definir("javascript:alert(1)"));

        Assert.Equal("setting_scheme_forbidden", erreur.Code);
    }

    [Fact]
    public void Toutes_LesClesSontAcceptees()
    {
        Assert.All(ClesReglage.Toutes, cle =>
        {
            var reglage = new ReglageSite(cle, "valeur");
            Assert.Equal(cle, reglage.Cle);
        });
    }
}
