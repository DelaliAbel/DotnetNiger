using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IAccountDeletionService
{
    Task<AccountDeletionRequestResponse> RequestDeletionAsync(Guid userId, string reason);
    Task CancelRequestAsync(Guid userId);
    Task<AccountDeletionRequestResponse?> GetLatestForUserAsync(Guid userId);
    Task<PaginatedResponse<AccountDeletionRequestResponse>> GetPendingAsync(int skip, int take);
    Task ApproveAsync(Guid requestId, Guid reviewerUserId);
    Task RejectAsync(Guid requestId, Guid reviewerUserId, string reason);
    Task<int> ExecuteApprovedAsync(Guid reviewerUserId, int batchSize = 100);
}
