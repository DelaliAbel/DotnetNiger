// Repository Identity: IRepository
using DotnetNiger.Identity.Application.Abstractions.Persistence;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat generique de repository.
public interface IRepository<TEntity> : IRepositoryPersistence<TEntity> where TEntity : class
{
}
