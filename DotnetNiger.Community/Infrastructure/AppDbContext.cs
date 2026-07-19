using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure;

/// <summary>Contexte Entity Framework Core pour la base de données de la communauté.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventMedia> EventMedias => Set<EventMedia>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceCategory> ResourceCategories => Set<ResourceCategory>();
    public DbSet<EventTag> EventTags => Set<EventTag>();
    public DbSet<ResourceTag> ResourceTags => Set<ResourceTag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<MemberSkill> MemberSkills => Set<MemberSkill>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    /// <summary>Applique les configurations d'entités et les filtres de requête globale.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Resource>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
    }
}
