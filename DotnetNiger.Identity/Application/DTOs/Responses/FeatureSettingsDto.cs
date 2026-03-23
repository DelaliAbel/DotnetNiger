namespace DotnetNiger.Identity.Application.DTOs.Responses;

public record FeatureSettingsDto
{
    public bool RegistrationEnabled { get; init; }
    public bool LoginEnabled { get; init; }
    public bool PasswordResetEnabled { get; init; }
    public bool EmailVerificationEnabled { get; init; }
    public bool AvatarUploadEnabled { get; init; }
    public bool ProfileDataExportEnabled { get; init; }
}
