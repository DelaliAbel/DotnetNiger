// DTO request Identity: RequestEmailVerificationRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Client.Models.Requests;

// Requete de renvoi du token de verification.
public class RequestEmailVerificationRequest
{
	[Required]
	public string Email { get; set; } = string.Empty;
}
