using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Domain.Entities;

namespace DotnetNiger.Api.Infrastructure.Data;

/// <summary>
/// Contexte Entity Framework principal de l'application, basé sur IdentityDbContext.
/// </summary>
public class DotnetNigerDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    /// <summary>
    /// Initialise une nouvelle instance du contexte de base de données.
    /// </summary>
    public DotnetNigerDbContext(DbContextOptions<DotnetNigerDbContext> options)
        : base(options) { }

    /// <summary>Table des permissions.</summary>
    public DbSet<Permission> Permissions => Set<Permission>();
    /// <summary>Table des services externes.</summary>
    public DbSet<ExternalService> ExternalServices => Set<ExternalService>();
    /// <summary>Table des journaux d'audit.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    /// <summary>Table des consentements utilisateurs.</summary>
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    /// <summary>Table de l'historique de connexions.</summary>
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    /// <summary>Table des catégories.</summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>Table des événements.</summary>
    public DbSet<Event> Events => Set<Event>();
    /// <summary>Table de liaison événements-tags.</summary>
    public DbSet<EventTag> EventTags => Set<EventTag>();
    /// <summary>Table des médias d'événements.</summary>
    public DbSet<EventMedia> EventMedias => Set<EventMedia>();
    /// <summary>Table des inscriptions aux événements.</summary>
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    /// <summary>Table des membres.</summary>
    public DbSet<Member> Members => Set<Member>();
    /// <summary>Table des compétences des membres.</summary>
    public DbSet<MemberSkill> MemberSkills => Set<MemberSkill>();
    /// <summary>Table des liens sociaux.</summary>
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    /// <summary>Table des articles.</summary>
    public DbSet<Post> Posts => Set<Post>();
    /// <summary>Table de liaison articles-catégories.</summary>
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    /// <summary>Table de liaison articles-tags.</summary>
    public DbSet<PostTag> PostTags => Set<PostTag>();
    /// <summary>Table des ressources.</summary>
    public DbSet<Resource> Resources => Set<Resource>();
    /// <summary>Table de liaison ressources-catégories.</summary>
    public DbSet<ResourceCategory> ResourceCategories => Set<ResourceCategory>();
    /// <summary>Table de liaison ressources-tags.</summary>
    public DbSet<ResourceTag> ResourceTags => Set<ResourceTag>();
    /// <summary>Table des tags.</summary>
    public DbSet<Tag> Tags => Set<Tag>();
    /// <summary>Table des commentaires.</summary>
    public DbSet<Comment> Comments => Set<Comment>();
    /// <summary>Table des signalements de commentaires.</summary>
    public DbSet<CommentReport> CommentReports => Set<CommentReport>();
    /// <summary>Table des intervenants.</summary>
    public DbSet<Speaker> Speakers => Set<Speaker>();
    /// <summary>Table des notifications.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();
    /// <summary>Table des messages de contact.</summary>
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    /// <summary>Table des abonnements newsletter.</summary>
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    /// <summary>Table des projets.</summary>
    public DbSet<Project> Projects => Set<Project>();
    /// <summary>Table des partenaires.</summary>
    public DbSet<Partner> Partners => Set<Partner>();
    /// <summary>Table des certificats.</summary>
    public DbSet<Certificate> Certificates => Set<Certificate>();
    /// <summary>Table des paramètres du site.</summary>
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    /// <summary>Table des demandes de suppression de compte.</summary>
    public DbSet<AccountDeletionRequest> AccountDeletionRequests => Set<AccountDeletionRequest>();
    /// <summary>Table des jetons de rafraîchissement.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Configure le modèle de données avec les indexes et contraintes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(DotnetNigerDbContext).Assembly);

        builder.Entity<ApplicationUser>(b =>
        {
            b.HasIndex(u => u.Email);
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

        // Refresh tokens : index par userId + expiration pour le nettoyage
        builder.Entity<RefreshToken>(b =>
        {
            b.HasIndex(r => r.TokenHash).IsUnique();
            b.HasIndex(r => new { r.UserId, r.ExpiresAt });
            b.Property(r => r.TokenHash).HasMaxLength(128);
        });
    }
}
