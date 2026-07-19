// DTO request Identity: RefreshTokenRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Client.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string? ClientId { get; set; }
}
