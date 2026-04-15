namespace DotnetNiger.Identity.Application.DTOs.Responses;

// Etat courant des parametres d'upload et de nettoyage.
// Current state of upload and cleanup settings.
public record FileUploadSettingsResponse
{
    public string Provider { get; init; } = string.Empty;
    public long MaxAvatarBytes { get; init; }
    public bool CleanupEnabled { get; init; }
    public int CleanupIntervalMinutes { get; init; }
    public int CleanupOrphanDays { get; init; }
}
