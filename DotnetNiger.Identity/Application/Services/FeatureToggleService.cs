using DotnetNiger.Identity.Application.Abstractions.Persistence;
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
    private readonly IAppSettingPersistence _appSettingRepository;

    public FeatureToggleService(IConfiguration configuration, IAppSettingPersistence appSettingRepository)
    {
        _appSettingRepository = appSettingRepository;
        _registrationEnabled = configuration.GetValue("Features:RegistrationEnabled", true);
        _loginEnabled = configuration.GetValue("Features:LoginEnabled", true);
        _passwordResetEnabled = configuration.GetValue("Features:PasswordResetEnabled", true);
        _emailVerificationEnabled = configuration.GetValue("Features:EmailVerificationEnabled", true);
        _avatarUploadEnabled = configuration.GetValue("Features:AvatarUploadEnabled", true);
        _profileDataExportEnabled = configuration.GetValue("Features:ProfileDataExportEnabled", true);

        ApplyOverrides();
    }

    public FeatureSettingsResponse GetCurrentSettings()
    {
        lock (_sync)
        {
            return ToDto();
        }
    }

    public FeatureSettingsResponse UpdateSettings(UpdateFeatureSettingsRequest request)
    {
        lock (_sync)
        {
            if (request.RegistrationEnabled.HasValue) _registrationEnabled = request.RegistrationEnabled.Value;
            if (request.LoginEnabled.HasValue) _loginEnabled = request.LoginEnabled.Value;
            if (request.PasswordResetEnabled.HasValue) _passwordResetEnabled = request.PasswordResetEnabled.Value;
            if (request.EmailVerificationEnabled.HasValue) _emailVerificationEnabled = request.EmailVerificationEnabled.Value;
            if (request.AvatarUploadEnabled.HasValue) _avatarUploadEnabled = request.AvatarUploadEnabled.Value;
            if (request.ProfileDataExportEnabled.HasValue) _profileDataExportEnabled = request.ProfileDataExportEnabled.Value;

            _appSettingRepository.SetValues(new Dictionary<string, string>
            {
                ["Features:RegistrationEnabled"] = _registrationEnabled.ToString(),
                ["Features:LoginEnabled"] = _loginEnabled.ToString(),
                ["Features:PasswordResetEnabled"] = _passwordResetEnabled.ToString(),
                ["Features:EmailVerificationEnabled"] = _emailVerificationEnabled.ToString(),
                ["Features:AvatarUploadEnabled"] = _avatarUploadEnabled.ToString(),
                ["Features:ProfileDataExportEnabled"] = _profileDataExportEnabled.ToString()
            });

            return ToDto();
        }
    }

    public bool IsRegistrationEnabled() => _registrationEnabled;
    public bool IsLoginEnabled() => _loginEnabled;
    public bool IsPasswordResetEnabled() => _passwordResetEnabled;
    public bool IsEmailVerificationEnabled() => _emailVerificationEnabled;
    public bool IsAvatarUploadEnabled() => _avatarUploadEnabled;
    public bool IsProfileDataExportEnabled() => _profileDataExportEnabled;

    private void ApplyOverrides()
    {
        var values = _appSettingRepository.GetByPrefix("Features:");
        if (TryGetBool(values, "Features:RegistrationEnabled", out var registrationEnabled)) _registrationEnabled = registrationEnabled;
        if (TryGetBool(values, "Features:LoginEnabled", out var loginEnabled)) _loginEnabled = loginEnabled;
        if (TryGetBool(values, "Features:PasswordResetEnabled", out var passwordResetEnabled)) _passwordResetEnabled = passwordResetEnabled;
        if (TryGetBool(values, "Features:EmailVerificationEnabled", out var emailVerificationEnabled)) _emailVerificationEnabled = emailVerificationEnabled;
        if (TryGetBool(values, "Features:AvatarUploadEnabled", out var avatarUploadEnabled)) _avatarUploadEnabled = avatarUploadEnabled;
        if (TryGetBool(values, "Features:ProfileDataExportEnabled", out var profileDataExportEnabled)) _profileDataExportEnabled = profileDataExportEnabled;
    }

    private static bool TryGetBool(IReadOnlyDictionary<string, string> values, string key, out bool result)
    {
        result = false;
        return values.TryGetValue(key, out var raw) && bool.TryParse(raw, out result);
    }

    private FeatureSettingsResponse ToDto()
    {
        return new FeatureSettingsResponse
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
