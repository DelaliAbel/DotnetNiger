// Service applicatif Identity: AdminService
using System.Security.Claims;
using System.Security.Cryptography;
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.External;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

// Logique d'administration des utilisateurs.
public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IApiKeyPersistence _apiKeyRepository;
    private readonly IAdminActionLogPersistence _adminActionLogRepository;
    private readonly IRefreshTokenPersistence _refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<FileUploadOptions> _fileUploadOptions;
    private readonly IFeatureToggleService _featureToggleService;
    private readonly IOptionsMonitor<AccountDeletionOptions> _accountDeletionOptions;
    private readonly IAppSettingPersistence _appSettingRepository;
    private readonly IConfiguration _configuration;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        IApiKeyPersistence apiKeyRepository,
        IAdminActionLogPersistence adminActionLogRepository,
        IRefreshTokenPersistence refreshTokenRepository,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<FileUploadOptions> fileUploadOptions,
        IFeatureToggleService featureToggleService,
        IOptionsMonitor<AccountDeletionOptions> accountDeletionOptions,
        IAppSettingPersistence appSettingRepository,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _apiKeyRepository = apiKeyRepository;
        _adminActionLogRepository = adminActionLogRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
        _fileUploadOptions = fileUploadOptions;
        _featureToggleService = featureToggleService;
        _accountDeletionOptions = accountDeletionOptions;
        _appSettingRepository = appSettingRepository;
        _configuration = configuration;
    }

    public async Task<PaginatedResponse<UserSummaryResponse>> GetUsersAsync(
        string? search,
        bool? isActive,
        bool? emailConfirmed,
        string? role,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? sortBy,
        string? sortDirection,
        int skip,
        int take)
    {
        var query = _userManager.Users.AsNoTracking();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(user =>
                EF.Functions.Like(user.UserName ?? string.Empty, $"%{term}%") ||
                EF.Functions.Like(user.Email ?? string.Empty, $"%{term}%") ||
                EF.Functions.Like(user.FullName ?? string.Empty, $"%{term}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (emailConfirmed.HasValue)
        {
            query = query.Where(user => user.EmailConfirmed == emailConfirmed.Value);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(user => user.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(user => user.CreatedAt <= createdTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(user => user.Id).ToList();
            query = query.Where(user => userIds.Contains(user.Id));
        }

        query = ApplyUserSorting(query, sortBy, sortDirection);

        var total = await query.CountAsync();
        var users = await query
            .Skip(skip)
            .Take(take)
            .Select(user => new UserSummaryResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            })
            .ToListAsync();

        return new PaginatedResponse<UserSummaryResponse>
        {
            Items = users,
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new IdentityException("User not found.", 404);
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        await LogAdminActionAsync("user.deleted", "user", user.Id.ToString(), "User deleted successfully");
    }

    public async Task SetUserActiveAsync(Guid userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new IdentityException("User not found.", 404);
        }

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        await LogAdminActionAsync("user.status.update", "user", user.Id.ToString(), $"isActive={isActive}");
    }

    public async Task<int> ForceLogoutUserSessionsAsync(Guid userId)
    {
        var revoked = await _refreshTokenRepository.RevokeActiveByUserIdAsync(userId, DateTime.UtcNow);
        await LogAdminActionAsync("user.force_logout", "user", userId.ToString(), $"revokedSessions={revoked}");
        return revoked;
    }

    public async Task<PaginatedResponse<ApiKeyAuditResponse>> GetApiKeysAsync(
        string? search,
        Guid? userId,
        bool? isActive,
        bool? expired,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? lastUsedFrom,
        DateTime? lastUsedTo,
        string? sortBy,
        string? sortDirection,
        int skip,
        int take)
    {
        var query = _apiKeyRepository.QueryWithUser();

        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(key =>
                EF.Functions.Like(key.Name, $"%{term}%") ||
                EF.Functions.Like(key.User.UserName ?? string.Empty, $"%{term}%") ||
                EF.Functions.Like(key.User.Email ?? string.Empty, $"%{term}%"));
        }

        if (userId.HasValue)
        {
            query = query.Where(key => key.UserId == userId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(key => key.IsActive == isActive.Value);
        }

        if (expired.HasValue)
        {
            query = expired.Value
                ? query.Where(key => key.ExpiresAt.HasValue && key.ExpiresAt.Value <= DateTime.UtcNow)
                : query.Where(key => !key.ExpiresAt.HasValue || key.ExpiresAt.Value > DateTime.UtcNow);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(key => key.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(key => key.CreatedAt <= createdTo.Value);
        }

        if (lastUsedFrom.HasValue)
        {
            query = query.Where(key => key.LastUsedAt.HasValue && key.LastUsedAt.Value >= lastUsedFrom.Value);
        }

        if (lastUsedTo.HasValue)
        {
            query = query.Where(key => key.LastUsedAt.HasValue && key.LastUsedAt.Value <= lastUsedTo.Value);
        }

        query = ApplyApiKeySorting(query, sortBy, sortDirection);

        var total = await query.CountAsync();
        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(key => new ApiKeyAuditResponse
            {
                Id = key.Id,
                Name = key.Name,
                IsActive = key.IsActive,
                CreatedAt = key.CreatedAt,
                LastUsedAt = key.LastUsedAt,
                ExpiresAt = key.ExpiresAt,
                IsExpired = key.ExpiresAt.HasValue && key.ExpiresAt.Value <= DateTime.UtcNow,
                UserId = key.UserId,
                Username = key.User.UserName ?? string.Empty,
                Email = key.User.Email ?? string.Empty
            })
            .ToListAsync();

        return new PaginatedResponse<ApiKeyAuditResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<ApiKeySecretResponse> RotateApiKeyAsync(Guid apiKeyId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId);
        if (apiKey == null)
        {
            throw new IdentityException("Api key not found.", 404);
        }

        if (!apiKey.IsActive)
        {
            throw new IdentityException("Api key is inactive.", 400);
        }

        var rawKey = GenerateKey();
        apiKey.Key = ApiKeyHasher.Hash(rawKey);
        apiKey.LastUsedAt = null;

        await _apiKeyRepository.UpdateAsync(apiKey);
        await LogAdminActionAsync("api_key.rotate", "api_key", apiKey.Id.ToString(), $"userId={apiKey.UserId}");

        return new ApiKeySecretResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Key = rawKey,
            IsActive = apiKey.IsActive,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    public async Task RevokeApiKeyAsync(Guid apiKeyId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId);
        if (apiKey == null)
        {
            throw new IdentityException("Api key not found.", 404);
        }

        apiKey.IsActive = false;
        await _apiKeyRepository.UpdateAsync(apiKey);
        await LogAdminActionAsync("api_key.revoke", "api_key", apiKey.Id.ToString(), $"userId={apiKey.UserId}");
    }

    public async Task RevokeUserApiKeysAsync(Guid userId)
    {
        var keys = await _apiKeyRepository.GetActiveByUserAsync(userId);
        if (keys.Count == 0)
        {
            return;
        }

        foreach (var key in keys)
        {
            key.IsActive = false;
            await _apiKeyRepository.UpdateAsync(key);
        }

        await LogAdminActionAsync("api_key.revoke_all", "user", userId.ToString(), $"count={keys.Count}");
    }

    public async Task<PaginatedResponse<AdminAuditLogResponse>> GetAdminAuditLogsAsync(
        Guid? adminUserId,
        string? action,
        string? targetType,
        DateTime? createdFrom,
        DateTime? createdTo,
        int skip,
        int take)
    {
        var query = _adminActionLogRepository.QueryWithAdminUser();

        if (adminUserId.HasValue)
        {
            query = query.Where(log => log.AdminUserId == adminUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var term = action.Trim();
            query = query.Where(log => EF.Functions.Like(log.Action, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            var term = targetType.Trim();
            query = query.Where(log => EF.Functions.Like(log.TargetType, $"%{term}%"));
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= createdTo.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(log => new AdminAuditLogResponse
            {
                Id = log.Id,
                AdminUserId = log.AdminUserId,
                AdminUsername = log.AdminUser.UserName ?? string.Empty,
                AdminEmail = log.AdminUser.Email ?? string.Empty,
                Action = log.Action,
                TargetType = log.TargetType,
                TargetId = log.TargetId,
                Details = log.Details,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<AdminAuditLogResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public Task<FileUploadSettingsResponse> GetFileUploadSettingsAsync()
    {
        var options = _fileUploadOptions.CurrentValue;
        var values = _appSettingRepository.GetByPrefix("FileUpload:");

        var cleanupEnabled = ReadBool(values, "FileUpload:CleanupEnabled", options.CleanupEnabled);
        var cleanupIntervalMinutes = ReadInt(values, "FileUpload:CleanupIntervalMinutes", options.CleanupIntervalMinutes);
        var cleanupOrphanDays = ReadInt(values, "FileUpload:CleanupOrphanDays", options.CleanupOrphanDays);

        return Task.FromResult(new FileUploadSettingsResponse
        {
            Provider = options.Provider,
            MaxAvatarBytes = options.MaxAvatarBytes,
            CleanupEnabled = cleanupEnabled,
            CleanupIntervalMinutes = cleanupIntervalMinutes,
            CleanupOrphanDays = cleanupOrphanDays
        });
    }

    public async Task<FileUploadSettingsResponse> UpdateFileUploadSettingsAsync(UpdateFileUploadSettingsRequest request)
    {
        var current = await GetFileUploadSettingsAsync();
        var options = _fileUploadOptions.CurrentValue;
        var updates = new Dictionary<string, string>();

        if (request.CleanupEnabled.HasValue)
        {
            options.CleanupEnabled = request.CleanupEnabled.Value;
            updates["FileUpload:CleanupEnabled"] = request.CleanupEnabled.Value.ToString();
        }
        else
        {
            options.CleanupEnabled = current.CleanupEnabled;
        }

        if (request.CleanupIntervalMinutes.HasValue)
        {
            options.CleanupIntervalMinutes = request.CleanupIntervalMinutes.Value;
            updates["FileUpload:CleanupIntervalMinutes"] = request.CleanupIntervalMinutes.Value.ToString();
        }
        else
        {
            options.CleanupIntervalMinutes = current.CleanupIntervalMinutes;
        }

        if (request.CleanupOrphanDays.HasValue)
        {
            options.CleanupOrphanDays = request.CleanupOrphanDays.Value;
            updates["FileUpload:CleanupOrphanDays"] = request.CleanupOrphanDays.Value.ToString();
        }
        else
        {
            options.CleanupOrphanDays = current.CleanupOrphanDays;
        }

        var adminId = GetAdminUserId();
        if (updates.Count > 0)
        {
            _appSettingRepository.SetValues(updates, adminId);
        }

        await LogAdminActionAsync(
            "UpdateFileUploadSettings",
            "Settings",
            "FileUpload",
            $"CleanupEnabled={options.CleanupEnabled}, IntervalMin={options.CleanupIntervalMinutes}, OrphanDays={options.CleanupOrphanDays}");

        return new FileUploadSettingsResponse
        {
            Provider = options.Provider,
            MaxAvatarBytes = options.MaxAvatarBytes,
            CleanupEnabled = options.CleanupEnabled,
            CleanupIntervalMinutes = options.CleanupIntervalMinutes,
            CleanupOrphanDays = options.CleanupOrphanDays
        };
    }

    public Task<FeatureSettingsResponse> GetFeatureSettingsAsync()
    {
        return Task.FromResult(_featureToggleService.GetCurrentSettings());
    }

    public async Task<FeatureSettingsResponse> UpdateFeatureSettingsAsync(UpdateFeatureSettingsRequest request)
    {
        var settings = _featureToggleService.UpdateSettings(request);

        await LogAdminActionAsync(
            "UpdateFeatureSettings",
            "Settings",
            "Features",
            $"Registration={settings.RegistrationEnabled}, Login={settings.LoginEnabled}, PasswordReset={settings.PasswordResetEnabled}, EmailVerification={settings.EmailVerificationEnabled}, AvatarUpload={settings.AvatarUploadEnabled}, ProfileDataExport={settings.ProfileDataExportEnabled}");

        return settings;
    }

    public Task<AccountDeletionSettingsResponse> GetAccountDeletionSettingsAsync()
    {
        var options = _accountDeletionOptions.CurrentValue;
        var values = _appSettingRepository.GetByPrefix("AccountDeletion:");

        var approvalWindowDays = ReadInt(values, "AccountDeletion:ApprovalWindowDays", options.ApprovalWindowDays);
        var defaultExecutionBatchSize = ReadInt(values, "AccountDeletion:DefaultExecutionBatchSize", options.DefaultExecutionBatchSize);
        var maxExecutionBatchSize = ReadInt(values, "AccountDeletion:MaxExecutionBatchSize", options.MaxExecutionBatchSize);

        return Task.FromResult(new AccountDeletionSettingsResponse
        {
            ApprovalWindowDays = approvalWindowDays,
            DefaultExecutionBatchSize = defaultExecutionBatchSize,
            MaxExecutionBatchSize = maxExecutionBatchSize
        });
    }

    public async Task<AccountDeletionSettingsResponse> UpdateAccountDeletionSettingsAsync(UpdateAccountDeletionSettingsRequest request, Guid? updatedByUserId)
    {
        var current = await GetAccountDeletionSettingsAsync();

        var approvalWindowDays = request.ApprovalWindowDays ?? current.ApprovalWindowDays;
        var defaultExecutionBatchSize = request.DefaultExecutionBatchSize ?? current.DefaultExecutionBatchSize;
        var maxExecutionBatchSize = request.MaxExecutionBatchSize ?? current.MaxExecutionBatchSize;

        if (defaultExecutionBatchSize > maxExecutionBatchSize)
        {
            throw new IdentityException("DefaultExecutionBatchSize cannot be greater than MaxExecutionBatchSize.", 400);
        }

        _appSettingRepository.SetValues(new Dictionary<string, string>
        {
            ["AccountDeletion:ApprovalWindowDays"] = approvalWindowDays.ToString(),
            ["AccountDeletion:DefaultExecutionBatchSize"] = defaultExecutionBatchSize.ToString(),
            ["AccountDeletion:MaxExecutionBatchSize"] = maxExecutionBatchSize.ToString()
        }, updatedByUserId);

        await LogAdminActionAsync(
            "UpdateAccountDeletionSettings",
            "Settings",
            "AccountDeletion",
            $"ApprovalWindowDays={approvalWindowDays}, DefaultExecutionBatchSize={defaultExecutionBatchSize}, MaxExecutionBatchSize={maxExecutionBatchSize}");

        return new AccountDeletionSettingsResponse
        {
            ApprovalWindowDays = approvalWindowDays,
            DefaultExecutionBatchSize = defaultExecutionBatchSize,
            MaxExecutionBatchSize = maxExecutionBatchSize
        };
    }

    public Task<AuthSettingsResponse> GetAuthSettingsAsync()
    {
        var defaultRole = _appSettingRepository.GetValue("Auth:DefaultRole")
            ?? _configuration["Auth:DefaultRole"]
            ?? "Member";

        var registerMaxAttempts = ReadIntFromSetting("Auth:RateLimit:RegisterMaxAttempts", 3);
        var loginMaxAttempts = ReadIntFromSetting("Auth:RateLimit:LoginMaxAttempts", 5);
        var passwordResetMaxAttempts = ReadIntFromSetting("Auth:RateLimit:PasswordResetMaxAttempts", 3);
        var rateLimitWindowMinutes = ReadIntFromSetting("Auth:RateLimit:WindowMinutes", 15);

        return Task.FromResult(new AuthSettingsResponse
        {
            DefaultRole = defaultRole,
            RegisterMaxAttempts = registerMaxAttempts,
            LoginMaxAttempts = loginMaxAttempts,
            PasswordResetMaxAttempts = passwordResetMaxAttempts,
            RateLimitWindowMinutes = rateLimitWindowMinutes
        });
    }

    public async Task<AuthSettingsResponse> UpdateAuthSettingsAsync(UpdateAuthSettingsRequest request, Guid? updatedByUserId)
    {
        var current = await GetAuthSettingsAsync();

        var defaultRole = request.DefaultRole?.Trim();
        if (string.IsNullOrWhiteSpace(defaultRole))
        {
            defaultRole = current.DefaultRole;
        }

        var allowedRoles = new[] { "Member", "Admin", "SuperAdmin" };
        if (!allowedRoles.Contains(defaultRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdentityException("Default role must be Member, Admin, or SuperAdmin.", 400);
        }

        if (!await _roleManager.RoleExistsAsync(defaultRole))
        {
            throw new IdentityException($"Role '{defaultRole}' does not exist.", 400);
        }

        var registerMaxAttempts = request.RegisterMaxAttempts ?? current.RegisterMaxAttempts;
        var loginMaxAttempts = request.LoginMaxAttempts ?? current.LoginMaxAttempts;
        var passwordResetMaxAttempts = request.PasswordResetMaxAttempts ?? current.PasswordResetMaxAttempts;
        var rateLimitWindowMinutes = request.RateLimitWindowMinutes ?? current.RateLimitWindowMinutes;

        _appSettingRepository.SetValues(new Dictionary<string, string>
        {
            ["Auth:DefaultRole"] = defaultRole,
            ["Auth:RateLimit:RegisterMaxAttempts"] = registerMaxAttempts.ToString(),
            ["Auth:RateLimit:LoginMaxAttempts"] = loginMaxAttempts.ToString(),
            ["Auth:RateLimit:PasswordResetMaxAttempts"] = passwordResetMaxAttempts.ToString(),
            ["Auth:RateLimit:WindowMinutes"] = rateLimitWindowMinutes.ToString()
        }, updatedByUserId);

        await LogAdminActionAsync(
            "UpdateAuthSettings",
            "Settings",
            "Auth",
            $"DefaultRole={defaultRole}, RegisterMaxAttempts={registerMaxAttempts}, LoginMaxAttempts={loginMaxAttempts}, PasswordResetMaxAttempts={passwordResetMaxAttempts}, WindowMinutes={rateLimitWindowMinutes}");

        return new AuthSettingsResponse
        {
            DefaultRole = defaultRole,
            RegisterMaxAttempts = registerMaxAttempts,
            LoginMaxAttempts = loginMaxAttempts,
            PasswordResetMaxAttempts = passwordResetMaxAttempts,
            RateLimitWindowMinutes = rateLimitWindowMinutes
        };
    }

    public Task<IReadOnlyList<OAuthProviderSettingsResponse>> GetOAuthProviderSettingsAsync()
    {
        var providers = new[] { "Google", "GitHub", "Microsoft" };
        var settings = providers
            .Select(ReadOAuthProviderSettings)
            .ToList();

        return Task.FromResult<IReadOnlyList<OAuthProviderSettingsResponse>>(settings);
    }

    public async Task<OAuthProviderSettingsResponse> UpdateOAuthProviderSettingsAsync(string provider, UpdateOAuthProviderSettingsRequest request, Guid? updatedByUserId)
    {
        var providerName = NormalizeOAuthProviderName(provider);
        if (providerName is null)
        {
            throw new IdentityException("Unsupported OAuth provider.", 400);
        }

        var current = ReadOAuthProviderSettings(providerName);

        var enabled = request.Enabled ?? current.Enabled;
        var clientId = request.ClientId?.Trim();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            clientId = current.ClientId;
        }

        var updates = new Dictionary<string, string>
        {
            [$"OAuth:{providerName}:Enabled"] = enabled.ToString(),
            [$"OAuth:{providerName}:ClientId"] = clientId
        };

        if (request.ClientSecret is not null)
        {
            updates[$"OAuth:{providerName}:ClientSecret"] = request.ClientSecret.Trim();
        }

        _appSettingRepository.SetValues(updates, updatedByUserId);

        await LogAdminActionAsync(
            "UpdateOAuthProviderSettings",
            "Settings",
            $"OAuth:{providerName}",
            $"Enabled={enabled}, ClientIdSet={!string.IsNullOrWhiteSpace(clientId)}, ClientSecretUpdated={request.ClientSecret is not null}");

        return ReadOAuthProviderSettings(providerName);
    }

    private async Task LogAdminActionAsync(string action, string targetType, string targetId, string details)
    {
        var adminUserId = GetAdminUserId();
        if (!adminUserId.HasValue)
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        var ip = context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var userAgent = context?.Request.Headers.UserAgent.ToString() ?? string.Empty;

        var log = new AdminActionLog
        {
            AdminUserId = adminUserId.Value,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            IpAddress = ip,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _adminActionLogRepository.AddAsync(log);
    }

    private Guid? GetAdminUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static IQueryable<ApplicationUser> ApplyUserSorting(IQueryable<ApplicationUser> query, string? sortBy, string? sortDirection)
    {
        var direction = sortDirection?.Trim().ToLowerInvariant();
        var ascending = direction == "asc";
        var key = sortBy?.Trim().ToLowerInvariant();

        return key switch
        {
            "username" => ascending ? query.OrderBy(user => user.UserName) : query.OrderByDescending(user => user.UserName),
            "email" => ascending ? query.OrderBy(user => user.Email) : query.OrderByDescending(user => user.Email),
            "lastlogin" => ascending ? query.OrderBy(user => user.LastLoginAt) : query.OrderByDescending(user => user.LastLoginAt),
            "createdat" => ascending ? query.OrderBy(user => user.CreatedAt) : query.OrderByDescending(user => user.CreatedAt),
            _ => query.OrderByDescending(user => user.CreatedAt)
        };
    }

    private static IQueryable<ApiKey> ApplyApiKeySorting(IQueryable<ApiKey> query, string? sortBy, string? sortDirection)
    {
        var direction = sortDirection?.Trim().ToLowerInvariant();
        var ascending = direction == "asc";
        var key = sortBy?.Trim().ToLowerInvariant();

        return key switch
        {
            "name" => ascending ? query.OrderBy(item => item.Name) : query.OrderByDescending(item => item.Name),
            "lastused" => ascending ? query.OrderBy(item => item.LastUsedAt) : query.OrderByDescending(item => item.LastUsedAt),
            "expiresat" => ascending ? query.OrderBy(item => item.ExpiresAt) : query.OrderByDescending(item => item.ExpiresAt),
            "username" => ascending ? query.OrderBy(item => item.User.UserName) : query.OrderByDescending(item => item.User.UserName),
            "createdat" => ascending ? query.OrderBy(item => item.CreatedAt) : query.OrderByDescending(item => item.CreatedAt),
            _ => query.OrderByDescending(item => item.CreatedAt)
        };
    }

    private static int ReadInt(IReadOnlyDictionary<string, string> values, string key, int fallback)
    {
        return values.TryGetValue(key, out var raw) && int.TryParse(raw, out var parsed)
            ? parsed
            : fallback;
    }

    private OAuthProviderSettingsResponse ReadOAuthProviderSettings(string provider)
    {
        var enabled = ReadBoolFromSetting($"OAuth:{provider}:Enabled", _configuration.GetValue($"OAuth:{provider}:Enabled", false));
        var clientId = _appSettingRepository.GetValue($"OAuth:{provider}:ClientId")
            ?? _configuration[$"OAuth:{provider}:ClientId"]
            ?? string.Empty;
        var clientSecret = _appSettingRepository.GetValue($"OAuth:{provider}:ClientSecret")
            ?? _configuration[$"OAuth:{provider}:ClientSecret"]
            ?? string.Empty;

        return new OAuthProviderSettingsResponse
        {
            Provider = provider,
            Enabled = enabled,
            ClientId = clientId,
            HasClientSecret = !string.IsNullOrWhiteSpace(clientSecret)
        };
    }

    private bool ReadBoolFromSetting(string key, bool fallback)
    {
        var value = _appSettingRepository.GetValue(key);
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static string? NormalizeOAuthProviderName(string provider)
    {
        return provider.Trim().ToLowerInvariant() switch
        {
            "google" => "Google",
            "github" => "GitHub",
            "microsoft" => "Microsoft",
            _ => null
        };
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key, bool fallback)
    {
        return values.TryGetValue(key, out var raw) && bool.TryParse(raw, out var parsed)
            ? parsed
            : fallback;
    }

    private int ReadIntFromSetting(string key, int fallback)
    {
        var value = _appSettingRepository.GetValue(key);
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var encoded = WebEncoders.Base64UrlEncode(bytes);
        return $"dnk_{encoded}";
    }
}
