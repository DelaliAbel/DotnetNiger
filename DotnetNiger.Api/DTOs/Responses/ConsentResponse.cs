namespace DotnetNiger.Api.DTOs.Responses;

public record ConsentResponse(
    string ConsentType,
    string ConsentVersion,
    bool Granted,
    DateTime CreatedAt);
