using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record CreateApiKeyRequest(
    [Required][StringLength(100, MinimumLength = 1)] string Name,
    string? Scopes,
    DateTime? ExpiresAt);
