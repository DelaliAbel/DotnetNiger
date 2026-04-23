namespace DotnetNiger.Community.Domain.Entities;

// Dynamic key-value settings persisted in DB and cached in memory.
public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; set; }
}
