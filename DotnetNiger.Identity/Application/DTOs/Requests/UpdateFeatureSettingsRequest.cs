namespace DotnetNiger.Identity.Application.DTOs.Requests;

public record UpdateFeatureSettingsRequest
{
    public bool? RegistrationEnabled { get; init; }
    public bool? LoginEnabled { get; init; }
    public bool? PasswordResetEnabled { get; init; }
    public bool? EmailVerificationEnabled { get; init; }
    public bool? AvatarUploadEnabled { get; init; }
    public bool? ProfileDataExportEnabled { get; init; }
}
