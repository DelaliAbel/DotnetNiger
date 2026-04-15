using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IAdminActionLogPersistence : IRepositoryPersistence<AdminActionLog>
{
    IQueryable<AdminActionLog> QueryWithAdminUser();
}
