namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de suppression définitive du compte (ré-authentification requise).</summary>
public record DeleteProfileRequest(
    string? Password);
