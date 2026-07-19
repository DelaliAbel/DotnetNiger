using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Seed;

public static class SeedCommunityContentService
{
    static readonly Guid AdminId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    public static List<Post> Seed(DotnetNigerDbContext db, DateTime now, List<Tag> tags, List<Category> cats)
    {
        var posts = SeedPostData(db, now);
        SeedPostRelations(db, posts, tags, cats);
        SeedCommentData(db, now, posts);
        return posts;
    }

    static List<Post> SeedPostData(DotnetNigerDbContext db, DateTime now)
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Introduction à Blazor WebAssembly", Slug = "introduction-blazor-webassembly", Content = "Blazor WebAssembly permet de construire des applications web interactives avec .NET directement dans le navigateur via WebAssembly. Dans cet article, nous explorons les bases, la création d'une première application, et les différences avec Blazor Server.", Excerpt = "Découvrez comment construire des applications web interactives avec Blazor WebAssembly et .NET.", CoverImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 245, PublishedAt = now.AddDays(-60), CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-58) },
            new() { Id = Guid.NewGuid(), Title = "Build .NET MAUI : Créez votre première app mobile", Slug = "build-net-maui-premiere-app", Content = "Avec .NET MAUI, créez des applications pour Android, iOS, Windows et macOS à partir d'une seule base de code C# et XAML. Guide pas à pas du projet au déploiement.", Excerpt = "Guide complet pour créer votre première application mobile avec .NET MAUI.", CoverImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "tutoriel", IsPublished = true, ViewCount = 189, PublishedAt = now.AddDays(-45), CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-44) },
            new() { Id = Guid.NewGuid(), Title = "Microservices avec .NET 9 et Ocelot", Slug = "microservices-dotnet9-ocelot", Content = "Les microservices sont devenus l'architecture de choix. Implémentez une passerelle API avec Ocelot : routage, authentification, rate limiting et circuit breaker avec Polly.", Excerpt = "Implémentez une architecture microservices robuste avec .NET 9 et la passerelle API Ocelot.", CoverImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 312, PublishedAt = now.AddDays(-30), CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-29) },
            new() { Id = Guid.NewGuid(), Title = "Introduction au Machine Learning avec Python", Slug = "introduction-machine-learning-python", Content = "Le machine learning transforme notre façon de traiter les données. Initiez-vous aux concepts fondamentaux avec Python : apprentissage supervisé, non supervisé, régression et classification.", Excerpt = "Les fondamentaux du machine learning avec Python : algorithmes et exemples pratiques.", CoverImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 178, PublishedAt = now.AddDays(-25), CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-24) },
            new() { Id = Guid.NewGuid(), Title = "Dockeriser votre application .NET", Slug = "dockeriser-application-dotnet", Content = "Docker simplifie le déploiement en encapsulant vos applications dans des conteneurs. Découvrez les Dockerfiles multi-stage, Docker Compose et les bonnes pratiques pour .NET.", Excerpt = "Apprenez à conteneuriser vos applications .NET avec Docker pour un déploiement fiable.", CoverImageUrl = "https://images.unsplash.com/photo-1605745341112-85968b19335b?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "tutoriel", IsPublished = true, ViewCount = 267, PublishedAt = now.AddDays(-20), CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-19) },
            new() { Id = Guid.NewGuid(), Title = "Sécuriser votre API avec OpenIddict", Slug = "securiser-api-openiddict", Content = "La sécurisation des API est cruciale. OpenIddict est la solution open-source de référence pour .NET : OAuth 2.0, OpenID Connect, jetons JWT et refresh tokens.", Excerpt = "Guide complet pour sécuriser vos API .NET avec OpenIddict et les standards OAuth 2.0.", CoverImageUrl = "https://images.unsplash.com/photo-1563013544-824ae1b704d3?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 156, PublishedAt = now.AddDays(-12), CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-11) },
            new() { Id = Guid.NewGuid(), Title = "EF Core : Performance et optimisation", Slug = "ef-core-performance-optimisation", Content = "Entity Framework Core est un ORM puissant, mais une mauvaise utilisation peut entraîner des problèmes de performance. Évitez le problème N+1, maîtrisez le chargement, créez des indexes efficaces.", Excerpt = "Optimisez les performances de vos requêtes Entity Framework Core avec ces techniques avancées.", CoverImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 134, PublishedAt = now.AddDays(-10), CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-9) },
            new() { Id = Guid.NewGuid(), Title = "CI/CD avec GitHub Actions pour .NET", Slug = "cicd-github-actions-dotnet", Content = "GitHub Actions permet d'automatiser vos pipelines CI/CD : build, test et déploiement de vos applications .NET. Configurez des workflows robustes pour la production.", Excerpt = "Automatisez le build, le test et le déploiement de vos applications .NET avec GitHub Actions.", CoverImageUrl = "https://images.unsplash.com/photo-1618401471353-b98afee0b2eb?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "tutoriel", IsPublished = true, ViewCount = 221, PublishedAt = now.AddDays(-7), CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-6) },
            new() { Id = Guid.NewGuid(), Title = "SignalR : Communication temps réel", Slug = "signalr-communication-temps-reel", Content = "SignalR permet d'ajouter des fonctionnalités temps réel : chat, notifications, tableaux de bord en direct. Découvrez les Hubs, les groupes et l'intégration avec Blazor.", Excerpt = "Implémentez des fonctionnalités temps réel dans vos applications .NET avec SignalR.", CoverImageUrl = "https://images.unsplash.com/photo-1552581234-26160f608093?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 167, PublishedAt = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-4) },
            new() { Id = Guid.NewGuid(), Title = "Kubernetes pour les développeurs .NET", Slug = "kubernetes-developpeurs-dotnet", Content = "Kubernetes orchestre vos conteneurs en production. Déployez vos applications .NET, gérez le scaling, les secrets, et les health probes.", Excerpt = "Déployez et gérez vos applications .NET sur Kubernetes : guide pratique.", CoverImageUrl = "https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=800", AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostType = "article", IsPublished = true, ViewCount = 198, PublishedAt = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-2) },
        };
        db.Posts.AddRange(posts);
        return posts;
    }

    static void SeedPostRelations(DotnetNigerDbContext db, List<Post> posts, List<Tag> tags, List<Category> cats)
    {
        var t = (string n) => tags.First(x => x.Name == n);
        var p = (int i) => posts[i];
        var catDev = cats[0]; var catMobile = cats[1]; var catData = cats[2];
        var catDevOps = cats[3]; var catSecurite = cats[5];

        db.PostCategories.AddRange(
            new PostCategory { PostId = p(0).Id, CategoryId = catDev.Id },
            new PostCategory { PostId = p(1).Id, CategoryId = catMobile.Id },
            new PostCategory { PostId = p(2).Id, CategoryId = catDev.Id },
            new PostCategory { PostId = p(2).Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = p(3).Id, CategoryId = catData.Id },
            new PostCategory { PostId = p(4).Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = p(5).Id, CategoryId = catSecurite.Id },
            new PostCategory { PostId = p(6).Id, CategoryId = catDev.Id },
            new PostCategory { PostId = p(7).Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = p(8).Id, CategoryId = catDev.Id },
            new PostCategory { PostId = p(9).Id, CategoryId = catDevOps.Id }
        );

        db.PostTags.AddRange(
            new PostTag { PostId = p(0).Id, TagId = t("blazor").Id }, new PostTag { PostId = p(0).Id, TagId = t("dotnet").Id },
            new PostTag { PostId = p(1).Id, TagId = t("csharp").Id }, new PostTag { PostId = p(1).Id, TagId = t("maui").Id },
            new PostTag { PostId = p(2).Id, TagId = t("dotnet").Id }, new PostTag { PostId = p(2).Id, TagId = t("api").Id }, new PostTag { PostId = p(2).Id, TagId = t("devops").Id },
            new PostTag { PostId = p(3).Id, TagId = t("python").Id }, new PostTag { PostId = p(3).Id, TagId = t("machine-learning").Id }, new PostTag { PostId = p(3).Id, TagId = t("ia").Id },
            new PostTag { PostId = p(4).Id, TagId = t("docker").Id }, new PostTag { PostId = p(4).Id, TagId = t("devops").Id },
            new PostTag { PostId = p(5).Id, TagId = t("dotnet").Id }, new PostTag { PostId = p(5).Id, TagId = t("api").Id }, new PostTag { PostId = p(5).Id, TagId = t("security").Id },
            new PostTag { PostId = p(6).Id, TagId = t("dotnet").Id }, new PostTag { PostId = p(6).Id, TagId = t("sql").Id },
            new PostTag { PostId = p(7).Id, TagId = t("devops").Id }, new PostTag { PostId = p(7).Id, TagId = t("docker").Id },
            new PostTag { PostId = p(8).Id, TagId = t("signalr").Id }, new PostTag { PostId = p(8).Id, TagId = t("dotnet").Id },
            new PostTag { PostId = p(9).Id, TagId = t("kubernetes").Id }, new PostTag { PostId = p(9).Id, TagId = t("devops").Id }, new PostTag { PostId = p(9).Id, TagId = t("docker").Id }
        );
    }

    static void SeedCommentData(DotnetNigerDbContext db, DateTime now, List<Post> posts)
    {
        var p = (int i) => posts[i];
        var c1 = Guid.NewGuid(); var c2 = Guid.NewGuid(); var c3 = Guid.NewGuid();
        db.Comments.AddRange(
            new Comment { Id = c1, Content = "Excellent article ! J'ai pu créer ma première app Blazor. Merci !", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(0).Id, CreatedAt = now.AddDays(-59) },
            new Comment { Id = c2, Content = "Très bon guide. Est-ce que vous pourriez approfondir sur l'intégration avec SignalR ?", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(0).Id, CreatedAt = now.AddDays(-57) },
            new Comment { Id = c3, Content = "Bien sûr ! Je prévois un article dédié à SignalR + Blazor la semaine prochaine.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(0).Id, ParentCommentId = c2, CreatedAt = now.AddDays(-56) },
            new Comment { Id = Guid.NewGuid(), Content = "L'architecture microservices est vraiment adaptée pour notre projet. Merci pour les conseils sur Ocelot.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(2).Id, CreatedAt = now.AddDays(-29) },
            new Comment { Id = Guid.NewGuid(), Content = "Docker a changé ma façon de développer. Excellent article introductif !", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(4).Id, CreatedAt = now.AddDays(-19) },
            new Comment { Id = Guid.NewGuid(), Content = "La sécurité est souvent négligée. Merci de rappeler les bonnes pratiques.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(5).Id, CreatedAt = now.AddDays(-11) },
            new Comment { Id = Guid.NewGuid(), Content = "Les problèmes N+1 m'ont déjà fait perdre des heures. Article très utile !", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(6).Id, CreatedAt = now.AddDays(-9) },
            new Comment { Id = Guid.NewGuid(), Content = "GitHub Actions est vraiment puissant. Merci pour ce guide pas à pas.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(7).Id, CreatedAt = now.AddDays(-6) },
            new Comment { Id = Guid.NewGuid(), Content = "SignalR c'est génial pour les notifications en temps réel. Je l'utilise pour mon dashboard.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(8).Id, CreatedAt = now.AddDays(-4) },
            new Comment { Id = Guid.NewGuid(), Content = "Kubernetes en production c'est un vrai game changer. Bel article !", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(9).Id, CreatedAt = now.AddDays(-2) },
            new Comment { Id = Guid.NewGuid(), Content = "Content que ça t'ait plu. Un article sur Kubernetes + Azure va suivre.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", PostId = p(9).Id, CreatedAt = now.AddDays(-1) }
        );
    }

    public static void SeedProjectData(DotnetNigerDbContext db, DateTime now)
    {
        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Title = "DotnetNiger Platform", Slug = "dotnetniger-platform", Description = "Plateforme communautaire pour les développeurs .NET au Niger : articles, événements, ressources et networking.", ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800", IsPublished = true, CreatedAt = now.AddDays(-100), UpdatedAt = now, RepositoryUrl = "https://github.com/dotnetniger/platform", DemoUrl = "https://dotnetniger.ne", Technologies = "Blazor, .NET 9, PostgreSQL, Docker" },
            new Project { Id = Guid.NewGuid(), Title = "Blazor Clean Architecture Template", Slug = "blazor-clean-architecture-template", Description = "Template Blazor WebAssembly avec Clean Architecture, authentification OpenIddict, gestion d'erreurs et logging structuré.", ImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=800", IsPublished = true, CreatedAt = now.AddDays(-80), UpdatedAt = now, RepositoryUrl = "https://github.com/dotnetniger/blazor-clean-template", Technologies = "Blazor WASM, .NET 9, OpenIddict" },
            new Project { Id = Guid.NewGuid(), Title = "Microservices .NET 9 Template", Slug = "microservices-dotnet9-template", Description = "Template complet pour architecture microservices : Gateway Ocelot, Service Discovery, RabbitMQ, Redis cache, Polly resilience et monitoring.", ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800", IsPublished = true, CreatedAt = now.AddDays(-60), UpdatedAt = now, RepositoryUrl = "https://github.com/dotnetniger/microservices-template", Technologies = ".NET 9, Ocelot, Docker, Kubernetes" }
        );
    }

    public static void SeedPartnerData(DotnetNigerDbContext db, DateTime now)
    {
        db.Partners.AddRange(
            new Partner { Id = Guid.NewGuid(), Name = "Microsoft", LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg", WebsiteUrl = "https://microsoft.com", Description = "Partenaire technologique principal", IsActive = true, SortOrder = 1, CreatedAt = now.AddDays(-365) },
            new Partner { Id = Guid.NewGuid(), Name = "GitHub", LogoUrl = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png", WebsiteUrl = "https://github.com", Description = "Plateforme de développement", IsActive = true, SortOrder = 2, CreatedAt = now.AddDays(-365) },
            new Partner { Id = Guid.NewGuid(), Name = "JetBrains", LogoUrl = "https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg", WebsiteUrl = "https://jetbrains.com", Description = "Outils de développement intelligents", IsActive = true, SortOrder = 3, CreatedAt = now.AddDays(-365) },
            new Partner { Id = Guid.NewGuid(), Name = "Docker", LogoUrl = "https://www.docker.com/wp-content/uploads/2022/03/Moby-logo.png", WebsiteUrl = "https://docker.com", Description = "Plateforme de conteneurisation", IsActive = true, SortOrder = 4, CreatedAt = now.AddDays(-365) },
            new Partner { Id = Guid.NewGuid(), Name = "Postman", LogoUrl = "https://www.postman.com/wp-content/uploads/2021/07/postman-logo.png", WebsiteUrl = "https://postman.com", Description = "Plateforme API", IsActive = true, SortOrder = 5, CreatedAt = now.AddDays(-365) }
        );
    }

public static void SeedSiteSettings(DotnetNigerDbContext db)
    {
        db.SiteSettings.AddRange(
            new SiteSetting { Id = "site.name", Key = "site.name", Value = "DotnetNiger", Type = "string", Description = "Nom du site", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "site.description", Key = "site.description", Value = "Communauté des développeurs .NET au Niger", Type = "string", Description = "Description du site", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "site.contact.email", Key = "site.contact.email", Value = "contact@dotnetniger.ne", Type = "string", Description = "Email de contact", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "site.social.github", Key = "site.social.github", Value = "https://github.com/dotnetniger", Type = "string", Description = "Lien GitHub", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "site.social.twitter", Key = "site.social.twitter", Value = "https://twitter.com/dotnetniger", Type = "string", Description = "Lien Twitter/X", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "site.social.linkedin", Key = "site.social.linkedin", Value = "https://linkedin.com/company/dotnetniger", Type = "string", Description = "Lien LinkedIn", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "features.comments.enabled", Key = "features.comments.enabled", Value = "true", Type = "boolean", Description = "Activer les commentaires", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "features.registration.enabled", Key = "features.registration.enabled", Value = "true", Type = "boolean", Description = "Activer l'inscription", UpdatedAt = DateTime.UtcNow },
            new SiteSetting { Id = "features.events.enabled", Key = "features.events.enabled", Value = "true", Type = "boolean", Description = "Activer les événements", UpdatedAt = DateTime.UtcNow }
        );
    }
}