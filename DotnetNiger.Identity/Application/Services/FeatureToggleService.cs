using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;

namespace DotnetNiger.Identity.Application.Services;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly object _sync = new();

    private bool _registrationEnabled = true;
    private bool _loginEnabled = true;
    private bool _passwordResetEnabled = true;
    private bool _emailVerificationEnabled = true;
    private bool _avatarUploadEnabled = true;
    private bool _profileDataExportEnabled = true;

    public FeatureToggleService(IConfiguration configuration)
    {
        _registrationEnabled = configuration.GetValue("Features:RegistrationEnabled", true);
        _loginEnabled = configuration.GetValue("Features:LoginEnabled", true);
        _passwordResetEnabled = configuration.GetValue("Features:PasswordResetEnabled", true);
        _emailVerificationEnabled = configuration.GetValue("Features:EmailVerificationEnabled", true);
        _avatarUploadEnabled = configuration.GetValue("Features:AvatarUploadEnabled", true);
        _profileDataExportEnabled = configuration.GetValue("Features:ProfileDataExportEnabled", true);
    }

    public FeatureSettingsDto GetCurrentSettings()
    {
        lock (_sync)
        {
            return ToDto();
        }
    }

    public FeatureSettingsDto UpdateSettings(UpdateFeatureSettingsRequest request)
    {
        lock (_sync)
        {
            if (request.RegistrationEnabled.HasValue) _registrationEnabled = request.RegistrationEnabled.Value;
            if (request.LoginEnabled.HasValue) _loginEnabled = request.LoginEnabled.Value;
            if (request.PasswordResetEnabled.HasValue) _passwordResetEnabled = request.PasswordResetEnabled.Value;
            if (request.EmailVerificationEnabled.HasValue) _emailVerificationEnabled = request.EmailVerificationEnabled.Value;
            if (request.AvatarUploadEnabled.HasValue) _avatarUploadEnabled = request.AvatarUploadEnabled.Value;
            if (request.ProfileDataExportEnabled.HasValue) _profileDataExportEnabled = request.ProfileDataExportEnabled.Value;

            return ToDto();
        }
    }

    public bool IsRegistrationEnabled() => _registrationEnabled;
    public bool IsLoginEnabled() => _loginEnabled;
    public bool IsPasswordResetEnabled() => _passwordResetEnabled;
    public bool IsEmailVerificationEnabled() => _emailVerificationEnabled;
    public bool IsAvatarUploadEnabled() => _avatarUploadEnabled;
    public bool IsProfileDataExportEnabled() => _profileDataExportEnabled;

    private FeatureSettingsDto ToDto()
    {
        return new FeatureSettingsDto
        {
            RegistrationEnabled = _registrationEnabled,
            LoginEnabled = _loginEnabled,
            PasswordResetEnabled = _passwordResetEnabled,
            EmailVerificationEnabled = _emailVerificationEnabled,
            AvatarUploadEnabled = _avatarUploadEnabled,
            ProfileDataExportEnabled = _profileDataExportEnabled
        };
    }
}
