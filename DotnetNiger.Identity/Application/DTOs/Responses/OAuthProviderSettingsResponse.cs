namespace DotnetNiger.Identity.Application.DTOs.Responses;

public record OAuthProviderSettingsResponse
{
    public string Provider { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public bool HasClientSecret { get; init; }
}
