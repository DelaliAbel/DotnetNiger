using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

// Requete de mise a jour des parametres de nettoyage d'avatars.
public record UpdateFileUploadSettingsRequest
{
    public bool? CleanupEnabled { get; init; }

    [Range(5, 10080, ErrorMessage = "CleanupIntervalMinutes doit etre entre 5 et 10080 (7 jours).")]
    public int? CleanupIntervalMinutes { get; init; }

    [Range(1, 365, ErrorMessage = "CleanupOrphanDays doit etre entre 1 et 365.")]
    public int? CleanupOrphanDays { get; init; }
}
