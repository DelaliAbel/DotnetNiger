using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class ChangeEmailRequest
{
    [Required(ErrorMessage = "La nouvelle adresse email est requise.")]
    [EmailAddress(ErrorMessage = "Adresse email invalide.")]
    public string NewEmail { get; set; } = string.Empty;
}
