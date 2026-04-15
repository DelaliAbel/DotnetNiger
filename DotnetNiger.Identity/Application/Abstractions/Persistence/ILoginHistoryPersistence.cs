using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface ILoginHistoryPersistence : IRepositoryPersistence<LoginHistory>
{
    Task<IReadOnlyList<LoginHistory>> GetForUserAsync(Guid userId, int skip, int take);
}
