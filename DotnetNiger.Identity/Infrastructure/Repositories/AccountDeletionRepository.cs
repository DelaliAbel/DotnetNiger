using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Domain.Enums;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Repository pour les demandes de suppression de compte.
public class AccountDeletionRepository : BaseRepository<AccountDeletionRequest>, IAccountDeletionRepository
{
    public AccountDeletionRepository(DotnetNigerIdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<bool> HasPendingForUserAsync(Guid userId)
    {
        return DbContext.AccountDeletionRequests
            .AnyAsync(request => request.UserId == userId && request.Status == AccountDeletionRequestStatus.Pending);
    }

    public Task<AccountDeletionRequest?> GetPendingForUserAsync(Guid userId)
    {
        return DbContext.AccountDeletionRequests
            .Where(request => request.UserId == userId && request.Status == AccountDeletionRequestStatus.Pending)
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync();
    }

    public Task<AccountDeletionRequest?> GetLatestForUserAsync(Guid userId)
    {
        return DbContext.AccountDeletionRequests
            .AsNoTracking()
            .Where(request => request.UserId == userId)
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync();
    }

    public Task<int> CountPendingAsync()
    {
        return DbContext.AccountDeletionRequests
            .AsNoTracking()
            .CountAsync(request => request.Status == AccountDeletionRequestStatus.Pending);
    }

    public async Task<IReadOnlyList<AccountDeletionRequest>> GetPendingAsync(int skip, int take)
    {
        return await DbContext.AccountDeletionRequests
            .AsNoTracking()
            .Include(request => request.User)
            .Where(request => request.Status == AccountDeletionRequestStatus.Pending)
            .OrderBy(request => request.RequestedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<AccountDeletionRequest?> GetByIdWithUserAsync(Guid requestId)
    {
        return DbContext.AccountDeletionRequests
            .Include(request => request.User)
            .FirstOrDefaultAsync(request => request.Id == requestId);
    }

    public async Task<IReadOnlyList<AccountDeletionRequest>> GetApprovedReadyToExecuteAsync(int batchSize, DateTime utcNow)
    {
        return await DbContext.AccountDeletionRequests
            .Include(request => request.User)
            .Where(request => request.Status == AccountDeletionRequestStatus.Approved && request.ScheduledDeletionAt <= utcNow)
            .OrderBy(request => request.ScheduledDeletionAt)
            .Take(batchSize)
            .ToListAsync();
    }
}
