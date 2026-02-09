using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Data;

public class DotnetNigerIdentityDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // Contexte Identity et entites metier.
    public DotnetNigerIdentityDbContext(DbContextOptions<DotnetNigerIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Liaisons utilisateur -> entites metier.
        builder.Entity<RefreshToken>()
            .HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId);

        builder.Entity<LoginHistory>()
            .HasOne(history => history.User)
            .WithMany(user => user.LoginHistories)
            .HasForeignKey(history => history.UserId);

        builder.Entity<SocialLink>()
            .HasOne(link => link.User)
            .WithMany(user => user.SocialLinks)
            .HasForeignKey(link => link.UserId);

        builder.Entity<ApiKey>()
            .HasOne(key => key.User)
            .WithMany()
            .HasForeignKey(key => key.UserId);
    }
}
