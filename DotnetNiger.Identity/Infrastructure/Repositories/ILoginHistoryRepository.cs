// Repository Identity: ILoginHistoryRepository
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat de repository pour l'historique des connexions.
public interface ILoginHistoryRepository : IRepository<LoginHistory>, ILoginHistoryPersistence
{
}
