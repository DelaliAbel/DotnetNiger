using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public record OAuthExchangeRequest
{
    [Required]
    [StringLength(32)]
    public string Provider { get; init; } = string.Empty;

    [Required]
    public string AccessToken { get; init; } = string.Empty;
}
