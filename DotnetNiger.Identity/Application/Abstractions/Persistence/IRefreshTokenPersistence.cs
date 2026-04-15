using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IRefreshTokenPersistence : IRepositoryPersistence<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(RefreshToken refreshToken);
    Task<int> RevokeActiveByUserIdAsync(Guid userId, DateTime revokedAtUtc);
}
