using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;

namespace DotnetNiger.DbManager;

/// <summary>Seed des données Community : catégories, tags, membres, posts, événements, ressources.</summary>
static class SeedCommunityService
{
    static readonly Guid AdminId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    /// <summary>Seed les données Community si aucune catégorie n'existe.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Categories.AnyAsync())
        {
            Console.WriteLine("   Community: already seeded, skipping.");
            return;
        }

        Console.WriteLine(">> Community: seeding data...");
        var now = DateTime.UtcNow;

        var (cats, tags) = SeedBase(db);
        SeedMembers(db, now);
        var posts = SeedCommunityContentService.Seed(db, now, tags, cats);
        SeedCommunityEventService.Seed(db, now, tags, cats);
        SeedCommunityResourceService.Seed(db, now, tags, cats);

        await db.SaveChangesAsync();
        Console.WriteLine("   Community: seed complete.");
    }

    static (List<Category>, List<Tag>) SeedBase(AppDbContext db)
    {
        var cats = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Développement Web", Slug = "developpement-web", Description = "Tout sur le développement web, du frontend au backend." },
            new() { Id = Guid.NewGuid(), Name = "Mobile", Slug = "mobile", Description = "Développement d'applications mobiles natives et hybrides." },
            new() { Id = Guid.NewGuid(), Name = "Data & IA", Slug = "data-ia", Description = "Data science, intelligence artificielle et machine learning." },
            new() { Id = Guid.NewGuid(), Name = "DevOps", Slug = "devops", Description = "Pratiques DevOps, CI/CD et infrastructure as code." },
            new() { Id = Guid.NewGuid(), Name = "Communauté", Slug = "communaute", Description = "Vie de la communauté, événements et actualités." },
            new() { Id = Guid.NewGuid(), Name = "Sécurité", Slug = "securite", Description = "Sécurité informatique, cybersécurité et bonnes pratiques." },
            new() { Id = Guid.NewGuid(), Name = "Cloud", Slug = "cloud", Description = "Services cloud, Azure, AWS et architecture cloud." },
            new() { Id = Guid.NewGuid(), Name = "Architecture", Slug = "architecture", Description = "Design patterns, clean architecture et bonnes pratiques." },
            new() { Id = Guid.NewGuid(), Name = "Open Source", Slug = "open-source", Description = "Projets open source, contributions et licences." },
        };
        db.Categories.AddRange(cats);

        var tags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "csharp", Slug = "csharp" },
            new() { Id = Guid.NewGuid(), Name = "dotnet", Slug = "dotnet" },
            new() { Id = Guid.NewGuid(), Name = "javascript", Slug = "javascript" },
            new() { Id = Guid.NewGuid(), Name = "python", Slug = "python" },
            new() { Id = Guid.NewGuid(), Name = "react", Slug = "react" },
            new() { Id = Guid.NewGuid(), Name = "azure", Slug = "azure" },
            new() { Id = Guid.NewGuid(), Name = "sql", Slug = "sql" },
            new() { Id = Guid.NewGuid(), Name = "docker", Slug = "docker" },
            new() { Id = Guid.NewGuid(), Name = "open-source", Slug = "open-source" },
            new() { Id = Guid.NewGuid(), Name = "tutoriel", Slug = "tutoriel" },
            new() { Id = Guid.NewGuid(), Name = "blazor", Slug = "blazor" },
            new() { Id = Guid.NewGuid(), Name = "api", Slug = "api" },
            new() { Id = Guid.NewGuid(), Name = "machine-learning", Slug = "machine-learning" },
            new() { Id = Guid.NewGuid(), Name = "ia", Slug = "ia" },
            new() { Id = Guid.NewGuid(), Name = "devops", Slug = "devops" },
            new() { Id = Guid.NewGuid(), Name = "kubernetes", Slug = "kubernetes" },
            new() { Id = Guid.NewGuid(), Name = "signalr", Slug = "signalr" },
            new() { Id = Guid.NewGuid(), Name = "security", Slug = "security" },
            new() { Id = Guid.NewGuid(), Name = "angular", Slug = "angular" },
            new() { Id = Guid.NewGuid(), Name = "typescript", Slug = "typescript" },
            new() { Id = Guid.NewGuid(), Name = "maui", Slug = "maui" },
            new() { Id = Guid.NewGuid(), Name = "html-css", Slug = "html-css" },
            new() { Id = Guid.NewGuid(), Name = "git", Slug = "git" },
        };
        db.Tags.AddRange(tags);

        return (cats, tags);
    }

    static Tag T(List<Tag> tags, string name) => tags.First(x => x.Name == name);

    static void SeedMembers(AppDbContext db, DateTime now)
    {
        var members = new[]
        {
            new Member { Id = AdminId, FullName = "Admin Plateforme", Email = "admin@dotnetniger.ne", Roles = "SuperAdmin,Admin,Collaborator", Bio = "Administrateur de la plateforme DotnetNiger. Passionné par .NET et l'écosystème open-source.", Country = "Niger", City = "Niamey", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", IsTeamMember = true, Position = "Fondateur & Admin", CreatedAt = now.AddDays(-365) },
        };
        db.Members.AddRange(members);

        db.SocialLinks.AddRange(
            new SocialLink { MemberId = AdminId, Platform = "GitHub", Url = "https://github.com/dotnetniger-admin" }
        );

        db.Set<MemberSkill>().AddRange(
            new MemberSkill { Id = Guid.NewGuid(), MemberId = AdminId, Name = "C#" },
            new MemberSkill { Id = Guid.NewGuid(), MemberId = AdminId, Name = ".NET" },
            new MemberSkill { Id = Guid.NewGuid(), MemberId = AdminId, Name = "Azure" },
            new MemberSkill { Id = Guid.NewGuid(), MemberId = AdminId, Name = "Architecture" }
        );
    }
}
