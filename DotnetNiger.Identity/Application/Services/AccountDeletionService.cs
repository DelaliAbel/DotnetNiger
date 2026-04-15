using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Domain.Enums;
using DotnetNiger.Identity.Infrastructure.External;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public class AccountDeletionService : IAccountDeletionService
{
    private readonly IAccountDeletionPersistence _accountDeletionRepository;
    private readonly IRefreshTokenPersistence _refreshTokenRepository;
    private readonly IAppSettingPersistence _appSettingRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AccountDeletionOptions _accountDeletionOptions;

    public AccountDeletionService(
        IAccountDeletionPersistence accountDeletionRepository,
        IRefreshTokenPersistence refreshTokenRepository,
        IAppSettingPersistence appSettingRepository,
        UserManager<ApplicationUser> userManager,
        IOptions<AccountDeletionOptions> accountDeletionOptions)
    {
        _accountDeletionRepository = accountDeletionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _appSettingRepository = appSettingRepository;
        _userManager = userManager;
        _accountDeletionOptions = accountDeletionOptions.Value;
    }

    public async Task<AccountDeletionRequestResponse> RequestDeletionAsync(Guid userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new IdentityException("User not found.", StatusCodes.Status404NotFound);
        }

        if (user.IsDeleted)
        {
            throw new IdentityException("Account already deleted.", StatusCodes.Status400BadRequest);
        }

        var pendingExists = await _accountDeletionRepository.HasPendingForUserAsync(userId);
        if (pendingExists)
        {
            throw new IdentityException("A pending account deletion request already exists.", StatusCodes.Status409Conflict);
        }

        var approvalWindowDays = ResolveIntSetting("AccountDeletion:ApprovalWindowDays", _accountDeletionOptions.ApprovalWindowDays, 1, 365);

        var request = new AccountDeletionRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Reason = reason?.Trim() ?? string.Empty,
            Status = AccountDeletionRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            ScheduledDeletionAt = DateTime.UtcNow.AddDays(approvalWindowDays)
        };

        await _accountDeletionRepository.AddAsync(request);

        return Map(request, user.Email ?? string.Empty);
    }

    public async Task CancelRequestAsync(Guid userId)
    {
        var request = await _accountDeletionRepository.GetPendingForUserAsync(userId);

        if (request is null)
        {
            throw new IdentityException("No pending account deletion request found.", StatusCodes.Status404NotFound);
        }

        request.Status = AccountDeletionRequestStatus.Cancelled;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewReason = "Cancelled by user.";
        await _accountDeletionRepository.UpdateAsync(request);
    }

    public async Task<AccountDeletionRequestResponse?> GetLatestForUserAsync(Guid userId)
    {
        var request = await _accountDeletionRepository.GetLatestForUserAsync(userId);

        if (request is null)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        return Map(request, user?.Email ?? string.Empty);
    }

    public async Task<PaginatedResponse<AccountDeletionRequestResponse>> GetPendingAsync(int skip, int take)
    {
        var total = await _accountDeletionRepository.CountPendingAsync();
        var items = await _accountDeletionRepository.GetPendingAsync(skip, take);

        return new PaginatedResponse<AccountDeletionRequestResponse>
        {
            Items = items.Select(r => Map(r, r.User.Email ?? string.Empty)).ToList(),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task ApproveAsync(Guid requestId, Guid reviewerUserId)
    {
        await EnsureSuperAdminAsync(reviewerUserId);

        var request = await _accountDeletionRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new IdentityException("Account deletion request not found.", StatusCodes.Status404NotFound);
        }

        if (request.Status != AccountDeletionRequestStatus.Pending)
        {
            throw new IdentityException("Only pending requests can be approved.", StatusCodes.Status400BadRequest);
        }

        request.Status = AccountDeletionRequestStatus.Approved;
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        await _accountDeletionRepository.UpdateAsync(request);
    }

    public async Task RejectAsync(Guid requestId, Guid reviewerUserId, string reason)
    {
        await EnsureSuperAdminAsync(reviewerUserId);

        var request = await _accountDeletionRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new IdentityException("Account deletion request not found.", StatusCodes.Status404NotFound);
        }

        if (request.Status != AccountDeletionRequestStatus.Pending)
        {
            throw new IdentityException("Only pending requests can be rejected.", StatusCodes.Status400BadRequest);
        }

        request.Status = AccountDeletionRequestStatus.Rejected;
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewReason = string.IsNullOrWhiteSpace(reason) ? "Rejected by super-admin." : reason.Trim();
        await _accountDeletionRepository.UpdateAsync(request);
    }

    public async Task<int> ExecuteApprovedAsync(Guid reviewerUserId, int batchSize = 100)
    {
        await EnsureSuperAdminAsync(reviewerUserId);

        var maxBatchSize = ResolveIntSetting("AccountDeletion:MaxExecutionBatchSize", _accountDeletionOptions.MaxExecutionBatchSize, 1, 5000);
        var defaultBatchSize = ResolveIntSetting("AccountDeletion:DefaultExecutionBatchSize", _accountDeletionOptions.DefaultExecutionBatchSize, 1, maxBatchSize);
        batchSize = batchSize < 1 ? defaultBatchSize : Math.Min(batchSize, maxBatchSize);

        var now = DateTime.UtcNow;
        var approvedRequests = await _accountDeletionRepository.GetApprovedReadyToExecuteAsync(batchSize, now);

        foreach (var request in approvedRequests)
        {
            var user = request.User;
            if (user != null)
            {
                user.IsActive = false;
                user.IsDeleted = true;
                user.DeletedAt = now;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                await _userManager.UpdateAsync(user);
            }

            await _refreshTokenRepository.RevokeActiveByUserIdAsync(request.UserId, now);

            request.Status = AccountDeletionRequestStatus.Executed;
            request.ExecutedAt = now;
            request.ReviewedByUserId ??= reviewerUserId;
            request.ReviewedAt ??= now;
            request.ReviewReason ??= "Executed after approval window.";

            await _accountDeletionRepository.UpdateAsync(request);
        }
        return approvedRequests.Count;
    }

    private async Task EnsureSuperAdminAsync(Guid reviewerUserId)
    {
        var reviewer = await _userManager.FindByIdAsync(reviewerUserId.ToString());
        if (reviewer is null || !await _userManager.IsInRoleAsync(reviewer, "SuperAdmin"))
        {
            throw new IdentityException("SuperAdmin role required.", StatusCodes.Status403Forbidden);
        }
    }

    private static AccountDeletionRequestResponse Map(AccountDeletionRequest request, string userEmail)
    {
        return new AccountDeletionRequestResponse
        {
            Id = request.Id,
            UserId = request.UserId,
            UserEmail = userEmail,
            Reason = request.Reason,
            Status = request.Status.ToString(),
            RequestedAt = request.RequestedAt,
            ScheduledDeletionAt = request.ScheduledDeletionAt,
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedAt = request.ReviewedAt,
            ReviewReason = request.ReviewReason,
            ExecutedAt = request.ExecutedAt
        };
    }

    private int ResolveIntSetting(string key, int fallback, int min, int max)
    {
        var value = _appSettingRepository.GetValue(key);
        if (int.TryParse(value, out var parsed))
        {
            return Math.Clamp(parsed, min, max);
        }

        return Math.Clamp(fallback, min, max);
    }
}
