namespace DotnetNiger.Domain.DTOs.Requests;

public record UpdateMemberRequest(
    string? DisplayName = null,
    string? Bio = null,
    string? Location = null,
    string? WebsiteUrl = null);
