using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class ConfirmChangeEmailRequest
{
    [Required(ErrorMessage = "La nouvelle adresse email est requise.")]
    [EmailAddress(ErrorMessage = "Adresse email invalide.")]
    public string NewEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code de confirmation est requis.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code fait 6 caractères.")]
    public string Code { get; set; } = string.Empty;
}
