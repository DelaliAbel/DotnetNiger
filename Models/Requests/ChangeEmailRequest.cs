// DTO request Identity: ChangeEmailRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

// Requete de changement d'email.
public class ChangeEmailRequest
{
	[Required]
	public string NewEmail { get; set; } = string.Empty;

	[Required]
	public string CurrentPassword { get; set; } = string.Empty;
}
