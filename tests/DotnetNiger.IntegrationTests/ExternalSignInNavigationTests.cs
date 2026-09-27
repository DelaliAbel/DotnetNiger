using Microsoft.AspNetCore.Authentication;
using DotnetNiger.Web.Authentication;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie le transport de la destination de retour et le choix du schéma de fournisseur.
/// </summary>
public class ExternalSignInNavigationTests
{
    [Theory]
    [InlineData("/communaute")]
    [InlineData("/")]
    [InlineData("/ressources/42?page=2")]
    public void Une_destination_interne_est_conservee(string destination)
    {
        var properties = ExternalSignInNavigation.BuildChallengeProperties(
            destination,
            "/connexion/google/retour");

        Assert.Equal(destination, ExternalSignInNavigation.ReadReturnUrl(properties));
    }

    [Theory]
    [InlineData("https://attaquant.example/steal")]
    [InlineData("//attaquant.example/steal")]
    [InlineData(@"/\attaquant.example")]
    [InlineData("javascript:alert(1)")]
    public void Une_destination_externe_est_ramenee_a_l_accueil(string destination)
    {
        // Le visiteur contrôle la valeur transmise au fournisseur, puis restituée au retour.
        // Sans ce filtrage, un lien forgé enverrait le membre ailleurs juste après sa
        // connexion, en laissant croire à un site de confiance.
        var properties = ExternalSignInNavigation.BuildChallengeProperties(
            destination,
            "/connexion/google/retour");

        Assert.Equal("/", ExternalSignInNavigation.ReadReturnUrl(properties));
    }

    [Fact]
    public void Une_destination_absente_vaut_accueil()
    {
        var properties = ExternalSignInNavigation.BuildChallengeProperties(null, "/connexion/google/retour");

        Assert.Equal("/", ExternalSignInNavigation.ReadReturnUrl(properties));
    }

    [Fact]
    public void Des_proprietes_vides_ne_donnent_aucune_destination()
    {
        Assert.Null(ExternalSignInNavigation.ReadReturnUrl(null));
        Assert.Null(ExternalSignInNavigation.ReadReturnUrl(new AuthenticationProperties()));
    }

    [Fact]
    public void L_adresse_de_retour_est_celle_du_fournisseur()
    {
        var properties = ExternalSignInNavigation.BuildChallengeProperties(
            "/",
            ExternalAccountEndpoints.CallbackPathFor("Google"));

        Assert.Equal("/connexion/google/retour", properties.RedirectUri);
    }

    [Theory]
    [InlineData("Google", null, ExternalSchemes.Google)]
    [InlineData("GitHub", null, ExternalSchemes.GitHub)]
    [InlineData("Microsoft", "common", ExternalSchemes.Microsoft)]
    [InlineData("Microsoft", null, ExternalSchemes.Microsoft)]
    [InlineData("Microsoft", "  ", ExternalSchemes.Microsoft)]
    [InlineData("Microsoft", "contoso.onmicrosoft.com", ExternalSchemes.MicrosoftOrganisational)]
    public void Le_schema_suit_le_locataire_microsoft(string provider, string? tenant, string attendu)
    {
        // Un compte professionnel est enregistré sous un autre schéma : interroger le
        // fournisseur par son seul nom produirait « aucun fournisseur ne correspond ».
        Assert.Equal(attendu, ExternalSchemes.ForProvider(provider, tenant));
    }
}
