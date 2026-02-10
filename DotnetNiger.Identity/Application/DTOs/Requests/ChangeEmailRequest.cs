using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public class ChangeEmailRequest
{
	[Required]
	public string NewEmail { get; set; } = string.Empty;

	[Required]
	public string CurrentPassword { get; set; } = string.Empty;
}
