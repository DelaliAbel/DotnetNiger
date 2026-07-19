namespace DotnetNiger.Domain.Entities;

public class SiteSetting
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
