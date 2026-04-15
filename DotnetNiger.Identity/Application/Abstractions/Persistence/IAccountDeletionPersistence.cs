using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IAccountDeletionPersistence : IRepositoryPersistence<AccountDeletionRequest>
{
    Task<bool> HasPendingForUserAsync(Guid userId);
    Task<AccountDeletionRequest?> GetPendingForUserAsync(Guid userId);
    Task<AccountDeletionRequest?> GetLatestForUserAsync(Guid userId);
    Task<int> CountPendingAsync();
    Task<IReadOnlyList<AccountDeletionRequest>> GetPendingAsync(int skip, int take);
    Task<AccountDeletionRequest?> GetByIdWithUserAsync(Guid requestId);
    Task<IReadOnlyList<AccountDeletionRequest>> GetApprovedReadyToExecuteAsync(int batchSize, DateTime utcNow);
}
