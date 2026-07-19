namespace DotnetNiger.Domain.DTOs.Responses;

public record ErrorResponse(
    string Message,
    string? Code = null,
    IList<string>? Errors = null);
