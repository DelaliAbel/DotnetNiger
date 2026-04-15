// Repository Identity: IRefreshTokenRepository
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat de repository pour les refresh tokens.
public interface IRefreshTokenRepository : IRepository<RefreshToken>, IRefreshTokenPersistence
{
}
