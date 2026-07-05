namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>DTO représentant un paramètre de configuration du site.</summary>
public class SiteSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
}
