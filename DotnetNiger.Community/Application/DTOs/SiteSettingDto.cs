namespace DotnetNiger.Community.Application.DTOs;

public class SiteSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
}

public class UpdateSiteSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

public class UpdateSiteSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = new();
}
