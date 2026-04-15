namespace DotnetNiger.Identity.Domain.Entities;

// Parametre applicatif modifiable a chaud par un super-admin.
public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; set; }
}
