using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;

namespace DotnetNiger.DbManager;

/// <summary>Seed des ressources, projets, partenaires, newsletters et paramètres Community.</summary>
static class SeedCommunityResourceService
{
    static readonly Guid AdminId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    /// <summary>Seed toutes les ressources, projets, partenaires et données annexes.</summary>
    public static void Seed(AppDbContext db, DateTime now, List<Tag> tags, List<Category> cats)
    {
        SeedResourceData(db, now, tags, cats);
        SeedProjectData(db, now);
        SeedPartnerData(db, now);
        SeedNewsletters(db, now);
        SeedSiteSettings(db);
    }

    static void SeedResourceData(AppDbContext db, DateTime now, List<Tag> tags, List<Category> cats)
    {
        var t = (string n) => tags.First(x => x.Name == n);
        var resources = new List<Resource>
        {
            new() { Id = Guid.NewGuid(), Title = "Cheatsheet C# 12", Slug = "cheatsheet-csharp-12", Description = "Aide-mémoire complet C# 12 : syntaxe, nouveaux patterns, records, primary constructors, collection expressions.", Url = "https://cheatsheet.example.com/csharp-12", ResourceType = "document", Level = "débutant", CreatedBy = AdminId, ViewCount = 450, CreatedAt = now.AddDays(-90) },
            new() { Id = Guid.NewGuid(), Title = "Template Blazor Clean Architecture", Slug = "template-blazor-clean-architecture", Description = "Template Blazor WebAssembly avec Clean Architecture, authentification OpenIddict, gestion d'erreurs et logging structuré.", Url = "https://github.com/dotnetniger/blazor-clean-template", ResourceType = "template", Level = "intermédiaire", CreatedBy = AdminId, ViewCount = 320, CreatedAt = now.AddDays(-80) },
            new() { Id = Guid.NewGuid(), Title = "Cours complet Entity Framework Core", Slug = "cours-complet-entity-framework-core", Description = "Formation vidéo de 10h : DbContext, migrations, relations, requêtes LINQ, chargement eager/explicit/lazy, performances et indexes.", Url = "https://learn.example.com/ef-core", ResourceType = "video", Level = "débutant", CreatedBy = AdminId, ViewCount = 280, CreatedAt = now.AddDays(-70) },
            new() { Id = Guid.NewGuid(), Title = "Guide Azure pour développeurs .NET", Slug = "guide-azure-developpeurs-dotnet", Description = "Ebook gratuit 150 pages : App Services, Azure Functions, Cosmos DB, Service Bus, DevOps et monitoring avec Application Insights.", Url = "https://azure.guide/dotnet", ResourceType = "ebook", Level = "intermédiaire", CreatedBy = AdminId, ViewCount = 195, CreatedAt = now.AddDays(-60) },
            new() { Id = Guid.NewGuid(), Title = "Workshop Docker Avancé", Slug = "workshop-docker-avance", Description = "Diapositives et exemples : multi-stage builds, Docker Compose, Swarm, Kubernetes, sécurisation des conteneurs et CI/CD Docker.", Url = "https://slides.example.com/docker-avance", ResourceType = "presentation", Level = "avancé", CreatedBy = AdminId, ViewCount = 210, CreatedAt = now.AddDays(-40) },
            new() { Id = Guid.NewGuid(), Title = "API RESTful avec .NET 9", Slug = "api-restful-dotnet-9", Description = "Créer des API RESTful : Minimal APIs, versioning, Swagger/OpenAPI, FluentValidation, tests d'intégration et déploiement.", Url = "https://api.guide/dotnet9", ResourceType = "document", Level = "intermédiaire", CreatedBy = AdminId, ViewCount = 380, CreatedAt = now.AddDays(-30) },
            new() { Id = Guid.NewGuid(), Title = "Sécuriser une API .NET avec JWT", Slug = "securiser-api-jwt-dotnet", Description = "Guide pratique : JWT, refresh tokens, gestion des rôles et permissions, rate limiting et protection contre les attaques courantes.", Url = "https://security.guide/dotnet-jwt", ResourceType = "document", Level = "intermédiaire", CreatedBy = AdminId, ViewCount = 290, CreatedAt = now.AddDays(-25) },
            new() { Id = Guid.NewGuid(), Title = "Guide Débutant C# : Les fondamentaux", Slug = "guide-debutant-csharp-fondamentaux", Description = "Introduction complète à C# : variables, types, classes, héritage, interfaces, génériques, LINQ, async/await et bonnes pratiques.", Url = "https://learn.example.com/csharp-debutant", ResourceType = "document", Level = "débutant", CreatedBy = AdminId, ViewCount = 510, CreatedAt = now.AddDays(-20) },
            new() { Id = Guid.NewGuid(), Title = "Template Microservices .NET 9", Slug = "template-microservices-dotnet9", Description = "Template complet pour architecture microservices : Gateway Ocelot, Service Discovery, RabbitMQ, Redis cache, Polly resilience et monitoring.", Url = "https://github.com/dotnetniger/microservices-template", ResourceType = "template", Level = "avancé", CreatedBy = AdminId, ViewCount = 175, CreatedAt = now.AddDays(-15) },
            new() { Id = Guid.NewGuid(), Title = "Ebook Blazor Avancé : Composites et Performance", Slug = "ebook-blazor-composants-performance", Description = "Ebook 200 pages : composants réutilisables, virtualisation, lazy loading, JavaScript interop, SignalR et optimisation des performances.", Url = "https://blazor-ebook.example.com/avance", ResourceType = "ebook", Level = "avancé", CreatedBy = AdminId, ViewCount = 140, CreatedAt = now.AddDays(-10) },
        };
        db.Resources.AddRange(resources);
        var res = (int i) => resources[i];

        db.ResourceCategories.AddRange(
            new ResourceCategory { ResourceId = res(0).Id, CategoryId = cats[0].Id },
            new ResourceCategory { ResourceId = res(1).Id, CategoryId = cats[0].Id },
            new ResourceCategory { ResourceId = res(2).Id, CategoryId = cats[0].Id },
            new ResourceCategory { ResourceId = res(3).Id, CategoryId = cats[6].Id },
            new ResourceCategory { ResourceId = res(4).Id, CategoryId = cats[3].Id },
            new ResourceCategory { ResourceId = res(5).Id, CategoryId = cats[0].Id },
            new ResourceCategory { ResourceId = res(6).Id, CategoryId = cats[5].Id },
            new ResourceCategory { ResourceId = res(7).Id, CategoryId = cats[0].Id },
            new ResourceCategory { ResourceId = res(8).Id, CategoryId = cats[7].Id },
            new ResourceCategory { ResourceId = res(9).Id, CategoryId = cats[0].Id }
        );
        db.ResourceTags.AddRange(
            new ResourceTag { ResourceId = res(0).Id, TagId = t("csharp").Id }, new ResourceTag { ResourceId = res(0).Id, TagId = t("dotnet").Id },
            new ResourceTag { ResourceId = res(1).Id, TagId = t("blazor").Id }, new ResourceTag { ResourceId = res(1).Id, TagId = t("dotnet").Id },
            new ResourceTag { ResourceId = res(2).Id, TagId = t("dotnet").Id }, new ResourceTag { ResourceId = res(2).Id, TagId = t("sql").Id },
            new ResourceTag { ResourceId = res(3).Id, TagId = t("azure").Id },
            new ResourceTag { ResourceId = res(4).Id, TagId = t("docker").Id }, new ResourceTag { ResourceId = res(4).Id, TagId = t("kubernetes").Id },
            new ResourceTag { ResourceId = res(5).Id, TagId = t("dotnet").Id }, new ResourceTag { ResourceId = res(5).Id, TagId = t("api").Id },
            new ResourceTag { ResourceId = res(6).Id, TagId = t("dotnet").Id }, new ResourceTag { ResourceId = res(6).Id, TagId = t("security").Id },
            new ResourceTag { ResourceId = res(7).Id, TagId = t("csharp").Id }, new ResourceTag { ResourceId = res(7).Id, TagId = t("dotnet").Id },
            new ResourceTag { ResourceId = res(8).Id, TagId = t("dotnet").Id }, new ResourceTag { ResourceId = res(8).Id, TagId = t("devops").Id }, new ResourceTag { ResourceId = res(8).Id, TagId = t("api").Id },
            new ResourceTag { ResourceId = res(9).Id, TagId = t("blazor").Id }, new ResourceTag { ResourceId = res(9).Id, TagId = t("dotnet").Id }
        );
    }

    static void SeedProjectData(AppDbContext db, DateTime now)
    {
        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Title = "Plateforme E-commerce DotnetNiger", Slug = "plateforme-e-commerce-dotnetniger", Description = "Plateforme e-commerce complète avec Blazor WebAssembly et .NET 9, intégration paiements Mobile Money (Orange Money, Wave), panier, wishlist et dashboard admin.", Url = "https://github.com/dotnetniger/ecommerce", GithubUrl = "https://github.com/dotnetniger/ecommerce", ImageUrl = "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=800", Technologies = "Blazor,.NET 9,Entity Framework,SQL Server,SignalR", Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme", IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-90) },
            new Project { Id = Guid.NewGuid(), Title = "API Gateway DotnetNiger", Slug = "api-gateway-dotnetniger", Description = "Passerelle API centralisée avec Ocelot : routage dynamique, authentification JWT, rate limiting, circuit breaker Polly et monitoring Prometheus/Grafana.", Url = "https://github.com/dotnetniger/gateway", GithubUrl = "https://github.com/dotnetniger/gateway", ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800", Technologies = "Ocelot,.NET 9,Polly,Prometheus,Swagger", Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme", IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-80) },
            new Project { Id = Guid.NewGuid(), Title = "Application Mobile Meetup", Slug = "application-mobile-meetup", Description = "App .NET MAUI pour les meetups DotnetNiger avec notifications push, agenda, speaker bios, QR code check-in et sondages en direct via SignalR.", Url = "https://github.com/dotnetniger/meetup-app", GithubUrl = "https://github.com/dotnetniger/meetup-app", ImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800", Technologies = ".NET MAUI,C#,SignalR,Azure Push", Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme", IsFeatured = false, IsPublished = true, CreatedAt = now.AddDays(-70) },
            new Project { Id = Guid.NewGuid(), Title = "Plateforme de Mentorat", Slug = "plateforme-de-mentorat", Description = "Plateforme connectant mentors et mentorés dans le domaine tech au Niger : matching intelligent, planning de sessions, suivi de progression et badges de certification.", Url = "https://github.com/dotnetniger/mentorship", GithubUrl = "https://github.com/dotnetniger/mentorship", ImageUrl = "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?w=800", Technologies = "Blazor,.NET 9,SignalR,Cosmos DB", Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme", IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-60) },
            new Project { Id = Guid.NewGuid(), Title = "CLI DotnetNiger", Slug = "cli-dotnetniger", Description = "Outil en ligne de commande pour la communauté : scaffold de projets, génération de code, vérification des bonnes pratiques et publication d'articles via terminal.", Url = "https://github.com/dotnetniger/cli", GithubUrl = "https://github.com/dotnetniger/cli", ImageUrl = "https://images.unsplash.com/photo-1629654297299-c8506221ca97?w=800", Technologies = "C#,System.CommandLine,.NET 9", Status = "beta", CreatedBy = AdminId, AuthorName = "Admin Plateforme", IsFeatured = false, IsPublished = true, CreatedAt = now.AddDays(-30) }
        );
    }

    static void SeedPartnerData(AppDbContext db, DateTime now)
    {
        db.Partners.AddRange(
            new Partner { Id = Guid.NewGuid(), Name = "Microsoft Niger", Slug = "microsoft-niger", Description = "Partenaire technologique officiel. Support Azure, licences et accompagnement technique pour la communauté .NET au Niger.", LogoUrl = "https://img.icons8.com/color/96/microsoft.png", WebsiteUrl = "https://www.microsoft.com/fr-fr/afrique/niger", PartnerType = "sponsor", SortOrder = 1, IsActive = true, CreatedAt = now.AddDays(-60) },
            new Partner { Id = Guid.NewGuid(), Name = "Oracle Niger", Slug = "oracle-niger", Description = "Support cloud et bases de données Oracle. Mise à disposition d'infrastructures pour les projets communautaires.", LogoUrl = "https://img.icons8.com/color/96/oracle-logo.png", WebsiteUrl = "https://www.oracle.com/afrique/niger", PartnerType = "sponsor", SortOrder = 2, IsActive = true, CreatedAt = now.AddDays(-55) },
            new Partner { Id = Guid.NewGuid(), Name = "GitHub Education", Slug = "github-education", Description = "Programme éducation pour les communautés : GitHub Classroom, Student Developer Pack et support pour les hackathons open source.", LogoUrl = "https://img.icons8.com/color/96/github.png", WebsiteUrl = "https://education.github.com", PartnerType = "community", SortOrder = 3, IsActive = true, CreatedAt = now.AddDays(-50) },
            new Partner { Id = Guid.NewGuid(), Name = "AWS Activate", Slug = "aws-activate", Description = "Crédits AWS et support infrastructure pour les startups membres de la communauté DotnetNiger via le programme AWS Activate.", LogoUrl = "https://img.icons8.com/color/96/amazon-web-services.png", WebsiteUrl = "https://aws.amazon.com/fr/activate", PartnerType = "sponsor", SortOrder = 4, IsActive = true, CreatedAt = now.AddDays(-40) },
            new Partner { Id = Guid.NewGuid(), Name = "Google for Startups", Slug = "google-for-startups", Description = "Accompagnement et crédits Google Cloud pour les projets innovants des membres de la communauté DotnetNiger.", LogoUrl = "https://img.icons8.com/color/96/google-logo.png", WebsiteUrl = "https://startup.google.com", PartnerType = "community", SortOrder = 5, IsActive = true, CreatedAt = now.AddDays(-30) }
        );
    }

    static void SeedNewsletters(AppDbContext db, DateTime now)
    {
    }

    static void SeedSiteSettings(AppDbContext db)
    {
        db.SiteSettings.AddRange(
            new SiteSetting { Key = "site.name", Value = "DotnetNiger", Type = "string", Description = "Nom du site" },
            new SiteSetting { Key = "site.description", Value = "Communauté des développeurs .NET au Niger", Type = "string", Description = "Description du site" },
            new SiteSetting { Key = "site.maintenance", Value = "false", Type = "boolean", Description = "Mode maintenance" },
            new SiteSetting { Key = "site.slogan", Value = "Ensemble, construisons l'avenir numérique du Niger", Type = "string", Description = "Slogan du site" },
            new SiteSetting { Key = "site.email.contact", Value = "contact@dotnetniger.com", Type = "string", Description = "Email de contact" }
        );
    }
}
