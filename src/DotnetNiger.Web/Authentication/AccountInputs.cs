using System.ComponentModel.DataAnnotations;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Formulaire de connexion par mot de passe.
/// </summary>
public sealed class LoginInput
{
    /// <summary>
    /// Adresse saisie.
    /// </summary>
    [Required(ErrorMessage = "Indiquez votre adresse e-mail.")]
    [EmailAddress(ErrorMessage = "Cette adresse n'est pas valide.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe saisi.
    /// </summary>
    [Required(ErrorMessage = "Indiquez votre mot de passe.")]
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Vrai si la session doit survivre à la fermeture du navigateur.
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// Destination interne demandée avant la connexion.
    /// </summary>
    /// <remarks>
    /// Transmise par un champ caché du formulaire : une destination placée dans la query de
    /// l'action disparaîtrait à la soumission. La valeur est systématiquement revérifiée
    /// avant d'y rediriger, pour ne pas servir de relais de redirection externe.
    /// </remarks>
    public string? Retour { get; set; }
}

/// <summary>
/// Formulaire de création de compte.
/// </summary>
public sealed class RegisterInput
{
    /// <summary>
    /// Prénom saisi.
    /// </summary>
    [Required(ErrorMessage = "Indiquez votre prénom.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Le prénom doit comporter au moins 2 caractères.")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Nom saisi.
    /// </summary>
    [Required(ErrorMessage = "Indiquez votre nom.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Le nom doit comporter au moins 2 caractères.")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Adresse saisie.
    /// </summary>
    [Required(ErrorMessage = "Indiquez votre adresse e-mail.")]
    [EmailAddress(ErrorMessage = "Cette adresse n'est pas valide.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe saisi.
    /// </summary>
    [Required(ErrorMessage = "Choisissez un mot de passe.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation du mot de passe.
    /// </summary>
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Les deux mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Formulaire de demande d'un lien de réinitialisation.
/// </summary>
public sealed class ForgotPasswordInput
{
    /// <summary>
    /// Adresse saisie.
    /// </summary>
    /// <remarks>
    /// Seule entrée acceptée par le formulaire : le message affiché après l'envoi est
    /// identique que le compte existe ou non, pour ne pas révéler quelles adresses sont
    /// inscrites.
    /// </remarks>
    [Required(ErrorMessage = "Indiquez votre adresse e-mail.")]
    [EmailAddress(ErrorMessage = "Cette adresse n'est pas valide.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Formulaire de choix d'un nouveau mot de passe après ouverture d'un lien.
/// </summary>
public sealed class ResetPasswordInput
{
    /// <summary>
    /// Jeton de réinitialisation transmis dans le lien du courriel.
    /// </summary>
    [Required(ErrorMessage = "Ce lien est incomplet. Demandez-en un nouveau.")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Nouveau mot de passe choisi.
    /// </summary>
    [Required(ErrorMessage = "Choisissez un mot de passe.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation du mot de passe.
    /// </summary>
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Les deux mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Adresse portée par le lien de réinitialisation.
    /// </summary>
    /// <remarks>
    /// Reprise du champ caché du formulaire, où elle a été placée à la soumission : elle
    /// disparaît ainsi de l'URL et des journaux. Elle reste ici pour que la validation
    /// s'exécute comme sur les autres entrées.
    /// </remarks>
    [Required(ErrorMessage = "Ce lien est incomplet. Demandez-en un nouveau.")]
    [EmailAddress(ErrorMessage = "Ce lien est incomplet. Demandez-en un nouveau.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Formulaire de choix de mot de passe après un retour de fournisseur externe.
/// </summary>
public sealed class SetPasswordInput
{
    /// <summary>
    /// Jeton protégé portant l'identité externe en attente.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe choisi.
    /// </summary>
    [Required(ErrorMessage = "Choisissez un mot de passe.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation du mot de passe.
    /// </summary>
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Les deux mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
