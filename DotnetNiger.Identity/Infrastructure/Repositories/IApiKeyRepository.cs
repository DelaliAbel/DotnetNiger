using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat de repository pour les cles API.
public interface IApiKeyRepository : IRepository<ApiKey>, IApiKeyPersistence
{
}
