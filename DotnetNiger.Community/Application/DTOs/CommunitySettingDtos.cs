namespace DotnetNiger.Community.Application.DTOs;

public record CommunitySettingDto(
    string Key,
    string Value,
    string? Description,
    string DataType,
    bool IsPublic,
    DateTime? UpdatedAt);

public record UpsertCommunitySettingRequest(
    string Key,
    string Value,
    string? Description,
    string DataType,
    bool IsPublic);
