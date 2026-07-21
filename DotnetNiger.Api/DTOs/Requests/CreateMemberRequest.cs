namespace DotnetNiger.Api.DTOs.Requests;

public record CreateMemberRequest(
    string DisplayName,
    string? Bio = null,
    string? Location = null,
    string? WebsiteUrl = null);