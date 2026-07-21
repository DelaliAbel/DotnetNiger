// DTO request Identity: ChangePasswordRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

// Requete de changement de mot de passe.
public class ChangePasswordRequest
{
	[Required]
	public string CurrentPassword { get; set; } = string.Empty;

	[Required]
	public string NewPassword { get; set; } = string.Empty;
}
