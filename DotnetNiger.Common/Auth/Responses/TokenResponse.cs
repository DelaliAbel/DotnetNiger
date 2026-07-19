using System.Text.Json.Serialization;

namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse contenant les tokens d'authentification.
/// </summary>
public class TokenResponse
{
    [JsonPropertyOrder(1)]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyOrder(3)]
    public int ExpiresIn { get; set; }

    [JsonPropertyOrder(4)]
    public string? RefreshToken { get; set; }

    [JsonPropertyOrder(5)]
    public string? IdToken { get; set; }

    [JsonPropertyOrder(6)]
    public string? Scope { get; set; }

    [JsonPropertyOrder(7)]
    public Guid UserId { get; set; }

    [JsonPropertyOrder(8)]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyOrder(9)]
    public Guid? TenantId { get; set; }

    [JsonPropertyOrder(10)]
    public IList<string> Roles { get; set; } = new List<string>();
}
