using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure;

/// <summary>
/// Contexte de la requête courante. Résolu par le middleware et injecté dans le DbContext
/// pour appliquer les filtres d'isolation multi-tenant.
/// </summary>
public class TenantContext
{
    public Guid? TenantId { get; set; }
}

/// <summary>
/// DbContext Identity avec support multi-tenant.
/// Les query filters isolent automatiquement les données par TenantId.
/// </summary>
public class IdentityDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    private readonly TenantContext _tenant;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, TenantContext tenant)
        : base(options) => _tenant = tenant;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<TenantClient> TenantClients => Set<TenantClient>();
    public DbSet<TenantApiKey> TenantApiKeys => Set<TenantApiKey>();
    public DbSet<ExternalService> ExternalServices => Set<ExternalService>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.TenantId).IsRequired();
            b.HasIndex(u => u.TenantId);
            b.HasQueryFilter(u => _tenant.TenantId == null || u.TenantId == _tenant.TenantId);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.Property(r => r.TenantId).IsRequired();
            b.HasIndex(r => r.TenantId);
            b.HasQueryFilter(r => _tenant.TenantId == null || r.TenantId == _tenant.TenantId);
        });

        builder.Entity<Tenant>(b =>
        {
            b.HasIndex(t => t.Slug).IsUnique();
        });

        builder.Entity<Permission>(b =>
        {
            b.HasIndex(p => new { p.TenantId, p.Name }).IsUnique();
            b.HasQueryFilter(p => _tenant.TenantId == null || p.TenantId == _tenant.TenantId);
        });

        builder.Entity<TenantClient>(b =>
        {
            b.HasIndex(c => c.ClientId).IsUnique();
            b.HasIndex(c => c.TenantId);
            b.HasQueryFilter(c => _tenant.TenantId == null || c.TenantId == _tenant.TenantId);
        });

builder.Entity<TenantApiKey>(b =>
        {
            b.HasIndex(k => k.PublicKey).IsUnique();
            b.HasIndex(k => k.TenantId);
            b.HasQueryFilter(k => _tenant.TenantId == null || k.TenantId == _tenant.TenantId);
        });

    builder.Entity<ExternalService>(b =>
        {
            b.HasIndex(s => s.Slug).IsUnique();
            b.HasIndex(s => s.TenantId);
            b.Property(s => s.Name).HasMaxLength(200);
            b.Property(s => s.Slug).HasMaxLength(200);
            b.Property(s => s.BaseUrl).HasMaxLength(500);
            b.Property(s => s.HealthEndpoint).HasMaxLength(200);
            b.Property(s => s.Status).HasConversion<string>().HasMaxLength(50);
            b.HasQueryFilter(s => _tenant.TenantId == null || s.TenantId == _tenant.TenantId);
        });

    builder.Entity<AuditLog>(b =>
        {
            b.HasIndex(a => a.CreatedAt);
            b.HasIndex(a => a.TenantId);
            b.HasIndex(a => a.UserId);
            b.HasIndex(a => new { a.EntityType, a.EntityId });
            b.HasQueryFilter(a => _tenant.TenantId == null || a.TenantId == _tenant.TenantId);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.HasMany<Permission>().WithMany()
                .UsingEntity<Dictionary<string, object>>("RolePermission",
                    j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasKey("RoleId", "PermissionId"));
        });
    }
}
