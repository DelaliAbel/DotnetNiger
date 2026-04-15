// Repository Identity: RefreshTokenRepository
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Repository pour les refresh tokens.
public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DotnetNigerIdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var hash = RefreshTokenGenerator.HashToken(token);
        return DbContext.RefreshTokens.FirstOrDefaultAsync(item => item.Token == hash);
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        DbContext.RefreshTokens.Update(refreshToken);
        await DbContext.SaveChangesAsync();
    }

    public async Task<int> RevokeActiveByUserIdAsync(Guid userId, DateTime revokedAtUtc)
    {
        var tokens = await DbContext.RefreshTokens
            .Where(item => item.UserId == userId && item.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = revokedAtUtc;
        }

        if (tokens.Count > 0)
        {
            await DbContext.SaveChangesAsync();
        }

        return tokens.Count;
    }
}
