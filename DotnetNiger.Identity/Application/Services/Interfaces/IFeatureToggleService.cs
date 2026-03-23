using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IFeatureToggleService
{
    FeatureSettingsDto GetCurrentSettings();
    FeatureSettingsDto UpdateSettings(UpdateFeatureSettingsRequest request);

    bool IsRegistrationEnabled();
    bool IsLoginEnabled();
    bool IsPasswordResetEnabled();
    bool IsEmailVerificationEnabled();
    bool IsAvatarUploadEnabled();
    bool IsProfileDataExportEnabled();
}
