using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Repository pour les journaux d'actions admin.
public class AdminActionLogRepository : BaseRepository<AdminActionLog>, IAdminActionLogRepository
{
    public AdminActionLogRepository(DotnetNigerIdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public IQueryable<AdminActionLog> QueryWithAdminUser()
    {
        return DbContext.AdminActionLogs
            .AsNoTracking()
            .Include(log => log.AdminUser)
            .AsQueryable();
    }
}
