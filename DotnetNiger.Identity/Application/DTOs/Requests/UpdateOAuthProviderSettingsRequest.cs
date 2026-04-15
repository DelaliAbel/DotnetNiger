using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public record UpdateOAuthProviderSettingsRequest
{
    public bool? Enabled { get; init; }

    [StringLength(256)]
    public string? ClientId { get; init; }

    [StringLength(1024)]
    public string? ClientSecret { get; init; }
}
