using Microsoft.EntityFrameworkCore;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Data;

public class CommunityDbContext : DbContext
{
    public CommunityDbContext(DbContextOptions<CommunityDbContext> options) : base(options)
    {
    }

    // DbSets pour les entités
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventMedia> EventMedias { get; set; } = null!;
    public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
    public DbSet<Partner> Partners { get; set; } = null!;
    public DbSet<PostCategory> PostCategories { get; set; } = null!;
    public DbSet<PostTag> PostTags { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectContributor> ProjectContributors { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<ResourceCategory> ResourceCategories { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<MemberSkill> MemberSkills { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration des relations et des clés primaires
        ConfigurePostEntity(modelBuilder);
        ConfigureCategoryEntity(modelBuilder);
        ConfigureCommentEntity(modelBuilder);
        ConfigureEventEntity(modelBuilder);
        ConfigurePartnerEntity(modelBuilder);
        ConfigureProjectEntity(modelBuilder);
        ConfigureResourceEntity(modelBuilder);
        ConfigureTagEntity(modelBuilder);
        ConfigureMemberEntity(modelBuilder);
    }

    private void ConfigurePostEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.PostCategories)
            .WithOne(pc => pc.Post)
            .HasForeignKey(pc => pc.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.PostTags)
            .WithOne(pt => pt.Post)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasIndex(p => p.Slug)
            .IsUnique();
    }

    private void ConfigureCategoryEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<PostCategory>()
            .HasKey(pc => new { pc.PostId, pc.CategoryId });

        modelBuilder.Entity<PostCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.PostCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureCommentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.PostId);
    }

    private void ConfigureEventEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Event>()
            .HasIndex(e => e.Slug)
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Medias)
            .WithOne(em => em.Event)
            .HasForeignKey(em => em.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Registrations)
            .WithOne(er => er.Event)
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigurePartnerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partner>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Partner>()
            .HasIndex(p => p.Slug)
            .IsUnique();
    }

    private void ConfigureProjectEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Contributors)
            .WithOne(pc => pc.Project)
            .HasForeignKey(pc => pc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureResourceEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.Slug)
            .IsUnique();

        modelBuilder.Entity<ResourceCategory>()
            .HasKey(rc => new { rc.ResourceId, rc.CategoryId });

        modelBuilder.Entity<ResourceCategory>()
            .HasOne(rc => rc.Category)
            .WithMany()
            .HasForeignKey(rc => rc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResourceCategory>()
            .HasOne(rc => rc.Resource)
            .WithMany(r => r.ResourceCategories)
            .HasForeignKey(rc => rc.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureTagEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureMemberEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>()
            .HasKey(tm => tm.Id);

        modelBuilder.Entity<Member>()
            .HasMany(tm => tm.Skills)
            .WithOne(tms => tms.Member)
            .HasForeignKey(tms => tms.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
