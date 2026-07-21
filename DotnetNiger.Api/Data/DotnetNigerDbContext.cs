using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Entities;

namespace DotnetNiger.Api.Data;

public class DotnetNigerDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public DotnetNigerDbContext(DbContextOptions<DotnetNigerDbContext> options)
        : base(options) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<OAuthClient> OAuthClients => Set<OAuthClient>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ExternalService> ExternalServices => Set<ExternalService>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventTag> EventTags => Set<EventTag>();
    public DbSet<EventMedia> EventMedias => Set<EventMedia>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberSkill> MemberSkills => Set<MemberSkill>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceCategory> ResourceCategories => Set<ResourceCategory>();
    public DbSet<ResourceTag> ResourceTags => Set<ResourceTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AccountDeletionRequest> AccountDeletionRequests => Set<AccountDeletionRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(DotnetNigerDbContext).Assembly);

        builder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Resource>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<ApplicationUser>(b =>
        {
            b.HasIndex(u => u.Email);
        });

        builder.Entity<OAuthClient>(b =>
        {
            b.HasIndex(c => c.ClientId).IsUnique();
        });

        builder.Entity<ApiKey>(b =>
        {
            b.HasIndex(k => k.PublicKey).IsUnique();
        });

        builder.Entity<ExternalService>(b =>
        {
            b.HasIndex(s => s.Slug).IsUnique();
            b.HasIndex(s => new { s.IsActive, s.Status });
            b.Property(s => s.Name).HasMaxLength(200);
            b.Property(s => s.Slug).HasMaxLength(200);
            b.Property(s => s.BaseUrl).HasMaxLength(500);
            b.Property(s => s.HealthEndpoint).HasMaxLength(200);
            b.Property(s => s.Status).HasConversion<string>().HasMaxLength(50);
        });

        builder.Entity<AuditLog>(b =>
        {
            b.HasIndex(a => a.CreatedAt);
            b.HasIndex(a => a.UserId);
            b.HasIndex(a => new { a.EntityType, a.EntityId });
        });

        builder.Entity<UserConsent>(b =>
        {
            b.HasIndex(c => new { c.UserId, c.CreatedAt });
            b.Property(c => c.ConsentType).HasMaxLength(50);
            b.Property(c => c.ConsentVersion).HasMaxLength(20);
        });

        builder.Entity<LoginHistory>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.UserId, e.CreatedAt });
            b.Property(e => e.IpAddress).HasMaxLength(50);
            b.Property(e => e.UserAgent).HasMaxLength(500);
            b.Property(e => e.Provider).HasMaxLength(50);
            b.Property(e => e.FailureReason).HasMaxLength(200);
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
