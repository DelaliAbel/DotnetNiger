using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IApiKeyPersistence : IRepositoryPersistence<ApiKey>
{
    Task<ApiKey?> GetByIdAndUserAsync(Guid apiKeyId, Guid userId);
    Task<ApiKey?> GetByIdWithUserAsync(Guid apiKeyId);
    Task<IReadOnlyList<ApiKey>> GetByUserAsync(Guid userId);
    Task<IReadOnlyList<ApiKey>> GetActiveByUserAsync(Guid userId);
    IQueryable<ApiKey> QueryWithUser();
}
