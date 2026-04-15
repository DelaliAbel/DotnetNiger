using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat de repository pour les demandes de suppression de compte.
public interface IAccountDeletionRepository : IRepository<AccountDeletionRequest>, IAccountDeletionPersistence
{
}
