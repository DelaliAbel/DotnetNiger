using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Api.Seed;

public static class SampleContent
{
    public static async Task SeedAsync(DotnetNigerDbContext db, Guid adminId)
    {
        var now = DateTime.UtcNow;

        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Web", Slug = "web", Description = "Developpement web" },
            new() { Id = Guid.NewGuid(), Name = "Mobile", Slug = "mobile", Description = "Developpement mobile" },
            new() { Id = Guid.NewGuid(), Name = "Cloud", Slug = "cloud", Description = "Cloud & DevOps" },
            new() { Id = Guid.NewGuid(), Name = "Data", Slug = "data", Description = "Data & IA" },
            new() { Id = Guid.NewGuid(), Name = "Architecture", Slug = "architecture", Description = "Architecture logicielle" },
        };
        db.Set<Category>().AddRange(categories);

        var tags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "C#", Slug = "csharp" },
            new() { Id = Guid.NewGuid(), Name = "Blazor", Slug = "blazor" },
            new() { Id = Guid.NewGuid(), Name = "ASP.NET", Slug = "aspnet" },
            new() { Id = Guid.NewGuid(), Name = "Entity Framework", Slug = "entity-framework" },
            new() { Id = Guid.NewGuid(), Name = "Azure", Slug = "azure" },
            new() { Id = Guid.NewGuid(), Name = "MAUI", Slug = "maui" },
            new() { Id = Guid.NewGuid(), Name = "SignalR", Slug = "signalr" },
            new() { Id = Guid.NewGuid(), Name = "Clean Architecture", Slug = "clean-architecture" },
        };
        db.Set<Tag>().AddRange(tags);

        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Meetup .NET 9",
                Slug = "meetup-net-9", Description = "Decouvrez les nouveautes de .NET 9",
                Location = "Niamey", EventType = "Meetup", Category = "Web",
                Status = EventStatus.Published, StartDate = now.AddDays(30),
                EndDate = now.AddDays(30), CoverImageUrl = "",
                CreatedBy = adminId, OrganizerId = adminId,
                OrganizerName = "Admin DotnetNiger", Capacity = 50,
                IsPublished = true, PublishedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Blazor Workshop",
                Slug = "blazor-workshop", Description = "Atelier pratique Blazor",
                Location = "En ligne", EventType = "Workshop", Category = "Web",
                Status = EventStatus.Published, StartDate = now.AddDays(60),
                EndDate = now.AddDays(61), CoverImageUrl = "",
                CreatedBy = adminId, OrganizerId = adminId,
                OrganizerName = "Admin DotnetNiger", Capacity = 30,
                IsPublished = true, PublishedAt = now
            },
        };
        db.Set<Event>().AddRange(events);

        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "DotnetNiger", Title = "Site communautaire DotnetNiger",
                Slug = "dotnetniger", Description = "Plateforme communautaire pour les devs .NET au Niger",
                Url = "https://github.com/dotnetniger", GithubUrl = "https://github.com/dotnetniger",
                Technologies = "ASP.NET Core, Blazor, EF Core, SQL Server",
                CreatedBy = adminId, AuthorName = "Admin DotnetNiger",
                IsPublished = true
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "OpenCode", Title = "OpenCode CLI",
                Slug = "opencode", Description = "Outil CLI pour le developpement assiste par IA",
                Url = "https://opencode.ai", GithubUrl = "https://github.com/opencode",
                Technologies = "C#, NET, AI",
                CreatedBy = adminId, AuthorName = "Admin DotnetNiger",
                IsPublished = true
            },
        };
        db.Set<Project>().AddRange(projects);

        var resources = new List<Resource>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Introduction a ASP.NET Core",
                Slug = "introduction-aspnet-core", Description = "Guide complet pour debuter avec ASP.NET Core",
                Url = "https://learn.microsoft.com/aspnet/core",
                ResourceType = "Article", Level = "Debutant",
                CreatedBy = adminId, AuthorId = adminId
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Blazor pour les debutants",
                Slug = "blazor-debutants", Description = "Apprenez a creer des apps web avec Blazor",
                Url = "https://learn.microsoft.com/blazor",
                ResourceType = "Cours", Level = "Intermediaire",
                CreatedBy = adminId, AuthorId = adminId
            },
        };
        db.Set<Resource>().AddRange(resources);

        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Bienvenue sur DotnetNiger",
                Slug = "bienvenue-dotnetniger",
                Content = "Nous sommes ravis de lancer la communaute .NET au Niger. Restez connectes pour les prochains evenements et ressources.",
                Excerpt = "Lancement de la communaute .NET au Niger",
                AuthorId = adminId, AuthorName = "Admin DotnetNiger",
                PostType = "Article", Status = PostStatus.Published,
                IsPublished = true, PublishedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Comment contribuer a un projet open source",
                Slug = "contribuer-open-source",
                Content = "Guide etape par etape pour faire votre premiere contribution a un projet open source .NET.",
                Excerpt = "Guide de contribution open source",
                AuthorId = adminId, AuthorName = "Admin DotnetNiger",
                PostType = "Tutoriel", Status = PostStatus.Published,
                IsPublished = true, PublishedAt = now
            },
        };
        db.Set<Post>().AddRange(posts);

        await db.SaveChangesAsync();
    }
}
