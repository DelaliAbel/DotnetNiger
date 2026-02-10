using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public class ForgotPasswordRequest
{
	[Required]
	public string Email { get; set; } = string.Empty;
}
