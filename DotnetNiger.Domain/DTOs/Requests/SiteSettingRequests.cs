namespace DotnetNiger.Domain.DTOs.Requests;

public class UpdateSiteSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

public class UpdateSiteSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = new();
}
