// Contrat applicatif Identity: IAdminService
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

// Contrat pour les operations d'administration.
public interface IAdminService
{
    Task<FileUploadSettingsResponse> GetFileUploadSettingsAsync();
    Task<FileUploadSettingsResponse> UpdateFileUploadSettingsAsync(UpdateFileUploadSettingsRequest request);
    Task<FeatureSettingsResponse> GetFeatureSettingsAsync();
    Task<FeatureSettingsResponse> UpdateFeatureSettingsAsync(UpdateFeatureSettingsRequest request);
    Task<AccountDeletionSettingsResponse> GetAccountDeletionSettingsAsync();
    Task<AccountDeletionSettingsResponse> UpdateAccountDeletionSettingsAsync(UpdateAccountDeletionSettingsRequest request, Guid? updatedByUserId);
    Task<AuthSettingsResponse> GetAuthSettingsAsync();
    Task<AuthSettingsResponse> UpdateAuthSettingsAsync(UpdateAuthSettingsRequest request, Guid? updatedByUserId);
    Task<IReadOnlyList<OAuthProviderSettingsResponse>> GetOAuthProviderSettingsAsync();
    Task<OAuthProviderSettingsResponse> UpdateOAuthProviderSettingsAsync(string provider, UpdateOAuthProviderSettingsRequest request, Guid? updatedByUserId);
    Task<PaginatedResponse<UserSummaryResponse>> GetUsersAsync(
        string? search,
        bool? isActive,
        bool? emailConfirmed,
        string? role,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? sortBy,
        string? sortDirection,
        int skip,
        int take);

    Task DeleteUserAsync(Guid userId);
    Task SetUserActiveAsync(Guid userId, bool isActive);
    Task<int> ForceLogoutUserSessionsAsync(Guid userId);
    Task<PaginatedResponse<ApiKeyAuditResponse>> GetApiKeysAsync(
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
        int take);
    Task<ApiKeySecretResponse> RotateApiKeyAsync(Guid apiKeyId);
    Task RevokeApiKeyAsync(Guid apiKeyId);
    Task RevokeUserApiKeysAsync(Guid userId);
    Task<PaginatedResponse<AdminAuditLogResponse>> GetAdminAuditLogsAsync(
        Guid? adminUserId,
        string? action,
        string? targetType,
        DateTime? createdFrom,
        DateTime? createdTo,
        int skip,
        int take);
}
