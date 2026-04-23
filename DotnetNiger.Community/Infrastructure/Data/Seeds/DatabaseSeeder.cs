using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Data.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedDataAsync(CommunityDbContext context)
    {
        // Vérifier si les données existent déjà
        if (context.Posts.IgnoreQueryFilters().Any()
            || context.Categories.Any()
            || context.Events.IgnoreQueryFilters().Any()
            || context.Projects.Any()
            || context.Resources.IgnoreQueryFilters().Any()
            || context.Partners.Any())
            return;

        // 1. Créer les catégories
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Tutoriels", Slug = "tutoriels", Description = "Tutoriels et guides pratiques" },
            new() { Id = Guid.NewGuid(), Name = ".NET", Slug = "dotnet", Description = "Articles sur .NET et C#" },
            new() { Id = Guid.NewGuid(), Name = "Actualités", Slug = "actualites", Description = "Dernières nouvelles de la communauté" },
            new() { Id = Guid.NewGuid(), Name = "Événements", Slug = "evenements", Description = "Informations sur les événements" }
        };
        await context.Categories.AddRangeAsync(categories);

        // 2. Créer les tags
        var tags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "C#", Slug = "csharp", PostCount = 0 },
            new() { Id = Guid.NewGuid(), Name = ".NET 8", Slug = "dotnet8", PostCount = 0 },
            new() { Id = Guid.NewGuid(), Name = "ASP.NET Core", Slug = "aspnetcore", PostCount = 0 },
            new() { Id = Guid.NewGuid(), Name = "Microservices", Slug = "microservices", PostCount = 0 },
            new() { Id = Guid.NewGuid(), Name = "Docker", Slug = "docker", PostCount = 0 }
        };
        await context.Tags.AddRangeAsync(tags);

        // 3. Créer des posts
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Guide complet de .NET 8",
                Slug = "guide-dotnet-8",
                Content = "Découvrez les nouvelles fonctionnalités de .NET 8...",
                Excerpt = "Les dernières innovations de .NET 8",
                CoverImageUrl = "https://via.placeholder.com/800x400",
                AuthorId = Guid.NewGuid(),
                PostType = "Blog",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ViewCount = 1250,
                SeoDescription = "Un guide complet sur les nouvelles fonctionnalités de .NET 8"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Microservices avec ASP.NET Core",
                Slug = "microservices-aspnetcore",
                Content = "Comment construire une architecture microservices robuste...",
                Excerpt = "Architecture microservices avec ASP.NET Core",
                CoverImageUrl = "https://via.placeholder.com/800x400",
                AuthorId = Guid.NewGuid(),
                PostType = "Tutorial",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ViewCount = 890,
                SeoDescription = "Un tutoriel détaillé sur l'architecture microservices"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Containerisation avec Docker et .NET",
                Slug = "docker-dotnet",
                Content = "Apprenez à containeriser vos applications .NET...",
                Excerpt = "Docker et .NET",
                CoverImageUrl = "https://via.placeholder.com/800x400",
                AuthorId = Guid.NewGuid(),
                PostType = "Tutorial",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ViewCount = 650,
                SeoDescription = "Guide pratique de containerisation Docker"
            }
        };
        await context.Posts.AddRangeAsync(posts);

        // 4. Créer des événements
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Conférence .NET 2026",
                Slug = "conference-dotnet-2026",
                Description = "La plus grande conférence .NET de l'année",
                Location = "Dakar, Sénégal",
                EventType = "Physical",
                StartDate = DateTime.UtcNow.AddDays(45),
                EndDate = DateTime.UtcNow.AddDays(47),
                CoverImageUrl = "https://via.placeholder.com/800x400",
                CreatedBy = Guid.NewGuid(),
                Capacity = 500,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "WebinaireMicroservices en Direct",
                Slug = "webinaire-microservices",
                Description = "Un webinaire sur les architectures microservices modernes",
                Location = "En ligne",
                EventType = "Online",
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
                CoverImageUrl = "https://via.placeholder.com/800x400",
                CreatedBy = Guid.NewGuid(),
                Capacity = 1000,
                MeetupLink = "https://zoom.us/j/...",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Atelier API Gateway avec Ocelot",
                Slug = "atelier-api-gateway-ocelot",
                Description = "Atelier pratique sur la mise en place d'une API Gateway avec Ocelot",
                Location = "En ligne",
                EventType = "Online",
                StartDate = DateTime.UtcNow.AddDays(14),
                EndDate = DateTime.UtcNow.AddDays(14).AddHours(2),
                CoverImageUrl = "https://via.placeholder.com/800x400",
                CreatedBy = Guid.NewGuid(),
                Capacity = 100,
                MeetupLink = "https://zoom.us/j/atelier-ocelot",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Session CI/CD pour .NET",
                Slug = "session-cicd-dotnet",
                Description = "Session dédiée aux pipelines CI/CD pour les applications .NET",
                Location = "En ligne",
                EventType = "Online",
                StartDate = DateTime.UtcNow.AddDays(4),
                EndDate = DateTime.UtcNow.AddDays(4).AddHours(2),
                CoverImageUrl = "https://via.placeholder.com/800x400",
                CreatedBy = Guid.NewGuid(),
                Capacity = 120,
                MeetupLink = "https://zoom.us/j/session-cicd",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
        await context.Events.AddRangeAsync(events);

        // 5. Créer des projets
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "DotnetNiger Framework",
                Slug = "dotnetniger-framework",
                Description = "Framework web moderne pour .NET",
                GitHubUrl = "https://github.com/dotnetniger/framework",
                OwnerId = Guid.NewGuid(),
                IsFeatured = true,
                Stars = 342,
                ContributorsCount = 12,
                Language = "C#",
                License = "MIT",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Community Management System",
                Slug = "cms-community",
                Description = "Système de gestion de contenu communautaire",
                GitHubUrl = "https://github.com/dotnetniger/cms",
                OwnerId = Guid.NewGuid(),
                IsFeatured = true,
                Stars = 156,
                ContributorsCount = 8,
                Language = "C#",
                License = "Apache-2.0",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Community CLI Tools",
                Slug = "community-cli-tools",
                Description = "Outils CLI pour automatiser les tâches communautaires",
                GitHubUrl = "https://github.com/dotnetniger/community-cli",
                OwnerId = Guid.NewGuid(),
                IsFeatured = true,
                Stars = 106,
                ContributorsCount = 5,
                Language = "C#",
                License = "Apache-2.0",
                CreatedAt = DateTime.UtcNow.AddDays(-80)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "DotnetNiger Learning Hub",
                Slug = "dotnetniger-learning-hub",
                Description = "Plateforme de parcours pédagogiques pour la communauté",
                GitHubUrl = "https://github.com/dotnetniger/learning-hub",
                OwnerId = Guid.NewGuid(),
                IsFeatured = true,
                Stars = 56,
                ContributorsCount = 20,
                Language = "C#",
                License = "Apache-2.0",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            }
        };
        await context.Projects.AddRangeAsync(projects);

        // 6. Créer des ressources
        var resources = new List<Resource>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Documentation officielle .NET",
                Slug = "doc-dotnet",
                Description = "Documentation complète de .NET",
                Url = "https://docs.microsoft.com/dotnet",
                ResourceType = "Documentation",
                Level = "Beginner",
                CreatedBy = Guid.NewGuid(),
                IsApproved = true,
                ViewCount = 5000,
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Tutoriel ASP.NET Core - freeCodeCamp",
                Slug = "tutorial-aspnetcore",
                Description = "Cours complet ASP.NET Core gratuit",
                Url = "https://www.freecodecamp.org/news/aspnet-core",
                ResourceType = "Course",
                Level = "Intermediate",
                CreatedBy = Guid.NewGuid(),
                IsApproved = true,
                ViewCount = 3200,
                CreatedAt = DateTime.UtcNow.AddDays(-80)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Architecture hexagonale en .NET",
                Slug = "architecture-hexagonale-dotnet",
                Description = "Ressource avancée sur l'architecture hexagonale en .NET",
                Url = "https://www.udemy.com/course/hexagonal-architecture-dotnet",
                ResourceType = "Course",
                Level = "Advanced",
                CreatedBy = Guid.NewGuid(),
                IsApproved = true,
                ViewCount = 2100,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code in C# - Udemy",
                Slug = "clean-code-csharp",
                Description = "Cours sur les bonnes pratiques de code",
                Url = "https://www.udemy.com/course/clean-code-csharp",
                ResourceType = "Course",
                Level = "Advanced",
                CreatedBy = Guid.NewGuid(),
                IsApproved = true,
                ViewCount = 200,
                CreatedAt = DateTime.UtcNow.AddDays(-70)
            }
        };
        await context.Resources.AddRangeAsync(resources);

        // 7. Créer des commentaires
        var comments = new List<Comment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PostId = posts[0].Id,
                UserId = Guid.NewGuid(),
                Content = "Excellent article ! Les explications sont très claires.",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                PostId = posts[0].Id,
                UserId = Guid.NewGuid(),
                Content = "Merci pour ce guide détaillé. Très utile pour les débutants !",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                PostId = posts[0].Id,
                UserId = Guid.NewGuid(),
                Content = "Merci pour ce guide détaillé. Très utile pour les débutants !",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                PostId = posts[0].Id,
                UserId = Guid.NewGuid(),
                Content = "Merci pour ce guide détaillé. Très utile pour les débutants !",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };
        await context.Comments.AddRangeAsync(comments);

        // 8. Créer des partenaires
        var partners = new List<Partner>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Microsoft",
                Slug = "microsoft",
                LogoUrl = "https://via.placeholder.com/200x100",
                Website = "https://microsoft.com",
                Description = "Partenaire premium",
                PartnerType = "Gold",
                Level = "Platinum",
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-365)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "JetBrains",
                Slug = "jetbrains",
                LogoUrl = "https://via.placeholder.com/200x100",
                Website = "https://jetbrains.com",
                Description = "Outils de développement",
                PartnerType = "Silver",
                Level = "Gold",
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-200)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "GitHub",
                Slug = "github",
                LogoUrl = "https://via.placeholder.com/200x100",
                Website = "https://github.com",
                Description = "Plateforme de collaboration et open source",
                PartnerType = "Silver",
                Level = "Gold",
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Docker",
                Slug = "docker",
                LogoUrl = "https://via.placeholder.com/200x100",
                Website = "https://www.docker.com",
                Description = "Conteneurisation et outils cloud-native",
                PartnerType = "Silver",
                Level = "Gold",
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-210)
            }
        };
        await context.Partners.AddRangeAsync(partners);

        // 9. Créer des membres d'équipe
        var Members = new List<TeamMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Mahamadou GARBA",
                Position = "Lead",
                Order = 1,
                IsActive = true,
                MembershipStatus = ApprovalStatus.Approved,
                RoleDescription = "Fondateur et lead technique",
                JoinedAt = DateTime.UtcNow.AddDays(-365)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Salif DIALLO",
                Position = "Organizer",
                Order = 2,
                IsActive = true,
                MembershipStatus = ApprovalStatus.Approved,
                RoleDescription = "Organisateur d'événements",
                JoinedAt = DateTime.UtcNow.AddDays(-300)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Hamidou SOW",
                Position = "Mentor",
                Order = 3,
                IsActive = true,
                MembershipStatus = ApprovalStatus.Approved,
                RoleDescription = "Mentor et formateur",
                JoinedAt = DateTime.UtcNow.AddDays(-250)
            }
        };
        await context.Members.AddRangeAsync(Members);

        // 10. Créer des skills pour les membres
        var skills = new List<TeamMemberSkill>
        {
            new() { Id = Guid.NewGuid(), MemberId = Members[0].Id, SkillName = "C#", Level = "Expert" },
            new() { Id = Guid.NewGuid(), MemberId = Members[0].Id, SkillName = ".NET", Level = "Expert" },
            new() { Id = Guid.NewGuid(), MemberId = Members[0].Id, SkillName = "ASP.NET Core", Level = "Expert" },
            new() { Id = Guid.NewGuid(), MemberId = Members[1].Id, SkillName = "Community", Level = "Expert" },
            new() { Id = Guid.NewGuid(), MemberId = Members[1].Id, SkillName = "Marketing", Level = "Advanced" },
            new() { Id = Guid.NewGuid(), MemberId = Members[2].Id, SkillName = "Mentoring", Level = "Expert" },
            new() { Id = Guid.NewGuid(), MemberId = Members[2].Id, SkillName = "Web Development", Level = "Advanced" }
        };
        await context.MemberSkills.AddRangeAsync(skills);

        // Lier les posts aux catégories
        var postCategories = new List<PostCategory>
        {
            new() { PostId = posts[0].Id, CategoryId = categories[1].Id },
            new() { PostId = posts[1].Id, CategoryId = categories[0].Id },
            new() { PostId = posts[2].Id, CategoryId = categories[0].Id }
        };
        await context.PostCategories.AddRangeAsync(postCategories);

        // Lier les posts aux tags
        var postTags = new List<PostTag>
        {
            new() { PostId = posts[0].Id, TagId = tags[0].Id },
            new() { PostId = posts[0].Id, TagId = tags[1].Id },
            new() { PostId = posts[1].Id, TagId = tags[2].Id },
            new() { PostId = posts[1].Id, TagId = tags[3].Id },
            new() { PostId = posts[2].Id, TagId = tags[4].Id }
        };
        await context.PostTags.AddRangeAsync(postTags);

        // Sauvegarder toutes les données
        await context.SaveChangesAsync();
    }
}
