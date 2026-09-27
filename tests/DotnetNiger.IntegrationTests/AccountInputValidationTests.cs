using DotnetNiger.Web.Authentication;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie que les formulaires statiques appliquent réellement les règles qu'ils déclarent.
/// </summary>
/// <remarks>
/// Les pages sont rendues en HTML et postent vers des points de terminaison d'API minimale :
/// la validation automatique n'y est pas branchée. Sans ce contrôle, une confirmation de mot
/// de passe divergente serait acceptée, puisque <c>UserManager</c> ne reçoit que le mot de
/// passe saisi.
/// </remarks>
public class AccountInputValidationTests
{
    [Fact]
    public void Validate_refuse_une_confirmation_divergente()
    {
        var input = new RegisterInput
        {
            FirstName = "Aminata",
            LastName = "Diallo",
            Email = "aminata@example.org",
            Password = "MotDePasseSolide1!",
            ConfirmPassword = "MotDePasseSolide2!",
        };

        var erreur = AccountInputValidation.Validate(input);

        Assert.Equal("Les deux mots de passe ne correspondent pas.", erreur);
    }

    [Fact]
    public void Validate_accepte_un_formulaire_coherent()
    {
        var input = new RegisterInput
        {
            FirstName = "Aminata",
            LastName = "Diallo",
            Email = "aminata@example.org",
            Password = "MotDePasseSolide1!",
            ConfirmPassword = "MotDePasseSolide1!",
        };

        Assert.Null(AccountInputValidation.Validate(input));
    }

    [Theory]
    [InlineData("", "Diallo", "aminata@example.org", "Indiquez votre prénom.")]
    [InlineData("A", "Diallo", "aminata@example.org", "Le prénom doit comporter au moins 2 caractères.")]
    [InlineData("Aminata", "", "aminata@example.org", "Indiquez votre nom.")]
    [InlineData("Aminata", "Diallo", "pas-une-adresse", "Cette adresse n'est pas valide.")]
    [InlineData("Aminata", "Diallo", "", "Indiquez votre adresse e-mail.")]
    public void Validate_refuse_un_champ_manquant_ou_invalide(
        string prenom,
        string nom,
        string courriel,
        string attendu)
    {
        var input = new RegisterInput
        {
            FirstName = prenom,
            LastName = nom,
            Email = courriel,
            Password = "MotDePasseSolide1!",
            ConfirmPassword = "MotDePasseSolide1!",
        };

        Assert.Equal(attendu, AccountInputValidation.Validate(input));
    }

    [Fact]
    public void Validate_refuse_une_confirmation_vide()
    {
        // Le champ de confirmation n'est pas obligatoire en soi, mais il doit correspondre :
        // une confirmation omise laisserait passer une faute de frappe.
        var input = new RegisterInput
        {
            FirstName = "Aminata",
            LastName = "Diallo",
            Email = "aminata@example.org",
            Password = "MotDePasseSolide1!",
        };

        Assert.Equal("Les deux mots de passe ne correspondent pas.", AccountInputValidation.Validate(input));
    }

    [Fact]
    public void Validate_refuse_une_confirmation_divergente_apres_fournisseur()
    {
        var input = new SetPasswordInput
        {
            Token = "jeton",
            Password = "MotDePasseSolide1!",
            ConfirmPassword = "AutreMotDePasse1!",
        };

        Assert.Equal("Les deux mots de passe ne correspondent pas.", AccountInputValidation.Validate(input));
    }

    [Fact]
    public void Validate_refuse_une_preuve_vide()
    {
        Assert.Equal(
            "Saisissez votre mot de passe actuel.",
            AccountInputValidation.Validate(new ProofInput { Token = "jeton" }));
    }

    [Fact]
    public void Validate_refuse_un_formulaire_absent()
    {
        Assert.Equal("Le formulaire est incomplet.", AccountInputValidation.Validate(null));
    }
}
