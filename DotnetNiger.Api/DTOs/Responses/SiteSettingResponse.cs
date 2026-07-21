namespace DotnetNiger.Api.DTOs.Responses;

public class SiteSettingResponse
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
}
