using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure;

public static class DbSeeder
{
    private static readonly Guid AdminId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
    private static readonly Guid MemberId1 = Guid.Parse("B1C2D3E4-F5A6-7890-BCDE-F12345678901");
    private static readonly Guid MemberId2 = Guid.Parse("C1D2E3F4-A5B6-7890-CDEF-123456789012");
    private static readonly Guid MemberId3 = Guid.Parse("D1E2F3A4-B5C6-7890-DEF1-234567890123");
    private static readonly Guid MemberId4 = Guid.Parse("E1F2A3B4-C5D6-7890-EF12-345678901234");
    private static readonly Guid MemberId5 = Guid.Parse("F1A2B3C4-D5E6-7890-F123-456789012345");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Posts.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ---- Categories ----
        var catDev = new Category { Id = Guid.NewGuid(), Name = "Développement Web", Slug = "developpement-web", Description = "Tout sur le développement web, du frontend au backend." };
        var catMobile = new Category { Id = Guid.NewGuid(), Name = "Mobile", Slug = "mobile", Description = "Développement d'applications mobiles natives et hybrides." };
        var catData = new Category { Id = Guid.NewGuid(), Name = "Data & IA", Slug = "data-ia", Description = "Data science, intelligence artificielle et machine learning." };
        var catDevOps = new Category { Id = Guid.NewGuid(), Name = "DevOps", Slug = "devops", Description = "Pratiques DevOps, CI/CD et infrastructure as code." };
        var catCommunaute = new Category { Id = Guid.NewGuid(), Name = "Communauté", Slug = "communaute", Description = "Vie de la communauté DotnetNiger, événements et actualités." };
        var catSecurite = new Category { Id = Guid.NewGuid(), Name = "Sécurité", Slug = "securite", Description = "Sécurité informatique, cybersécurité et bonnes pratiques." };
        db.Categories.AddRange(catDev, catMobile, catData, catDevOps, catCommunaute, catSecurite);

        // ---- Tags ----
        var tagCsharp = new Tag { Id = Guid.NewGuid(), Name = "csharp", Slug = "csharp" };
        var tagDotnet = new Tag { Id = Guid.NewGuid(), Name = "dotnet", Slug = "dotnet" };
        var tagJs = new Tag { Id = Guid.NewGuid(), Name = "javascript", Slug = "javascript" };
        var tagPython = new Tag { Id = Guid.NewGuid(), Name = "python", Slug = "python" };
        var tagReact = new Tag { Id = Guid.NewGuid(), Name = "react", Slug = "react" };
        var tagAzure = new Tag { Id = Guid.NewGuid(), Name = "azure", Slug = "azure" };
        var tagSql = new Tag { Id = Guid.NewGuid(), Name = "sql", Slug = "sql" };
        var tagDocker = new Tag { Id = Guid.NewGuid(), Name = "docker", Slug = "docker" };
        var tagOpensource = new Tag { Id = Guid.NewGuid(), Name = "open-source", Slug = "open-source" };
        var tagTutoriel = new Tag { Id = Guid.NewGuid(), Name = "tutoriel", Slug = "tutoriel" };
        var tagBlazor = new Tag { Id = Guid.NewGuid(), Name = "blazor", Slug = "blazor" };
        var tagApi = new Tag { Id = Guid.NewGuid(), Name = "api", Slug = "api" };
        var tagMl = new Tag { Id = Guid.NewGuid(), Name = "machine-learning", Slug = "machine-learning" };
        var tagIa = new Tag { Id = Guid.NewGuid(), Name = "ia", Slug = "ia" };
        var tagDevops = new Tag { Id = Guid.NewGuid(), Name = "devops", Slug = "devops" };
        db.Tags.AddRange(tagCsharp, tagDotnet, tagJs, tagPython, tagReact, tagAzure, tagSql, tagDocker, tagOpensource, tagTutoriel, tagBlazor, tagApi, tagMl, tagIa, tagDevops);

        // ---- Posts ----
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Introduction à Blazor WebAssembly", Slug = "introduction-blazor-webassembly",
                Content = "Blazor WebAssembly est un framework qui permet de construire des applications web interactives avec .NET. Dans cet article, nous allons explorer les bases de Blazor, comment créer votre première application, et les différences avec Blazor Server.\n\n## Qu'est-ce que Blazor WebAssembly ?\nBlazor WebAssembly exécute du code C# directement dans le navigateur via WebAssembly. Cela signifie que vous pouvez utiliser les mêmes langages et outils .NET pour le frontend et le backend.\n\n## Premiers pas\nPour commencer, installez le SDK .NET 9 et créez un nouveau projet avec `dotnet new blazorwasm`. Vous obtiendrez une application prête à l'emploi avec des exemples de composants.\n\n## Avantages\n- Partage de code entre client et serveur\n- Performance des applications natives\n- Écosystème .NET complet\n- Sécurité renforcée",
                Excerpt = "Découvrez comment construire des applications web interactives avec Blazor WebAssembly et .NET.",
                CoverImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 245,
                PublishedAt = now.AddDays(-60), CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-58)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Build .NET MAUI : Créez votre première app mobile", Slug = "build-net-maui-premiere-app",
                Content = ".NET MAUI est le successeur de Xamarin.Forms pour créer des applications mobiles multiplateformes. Cet article vous guide pas à pas.\n\n## Pourquoi .NET MAUI ?\nAvec .NET MAUI, vous pouvez créer des applications pour Android, iOS, Windows et macOS à partir d'une seule base de code C# et XAML.\n\n## Création d'un projet\n```bash\ndotnet new maui -n MonApp\ncd MonApp\ndotnet build\ndotnet run\n```\n\n## Architecture MVVM\nLe pattern MVVM est recommandé pour structurer vos applications MAUI. Il sépare la logique métier de l'interface utilisateur pour une meilleure maintenabilité.",
                Excerpt = "Guide complet pour créer votre première application mobile avec .NET MAUI.",
                CoverImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "tutoriel", IsPublished = true, ViewCount = 189,
                PublishedAt = now.AddDays(-45), CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-44)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Microservices avec .NET 9 et Ocelot", Slug = "microservices-dotnet9-ocelot",
                Content = "Les microservices sont devenus l'architecture de choix pour les applications d'entreprise. Dans cet article, nous allons voir comment implémenter une passerelle API avec Ocelot.\n\n## Qu'est-ce qu'une API Gateway ?\nUne API Gateway est un point d'entrée unique pour tous vos microservices. Elle gère le routage, l'authentification, le rate limiting et l'agrégation des réponses.\n\n## Configuration d'Ocelot\nOcelot est une passerelle API open source pour .NET. Voici un exemple de configuration :\n\n```json\n{\n  \"Routes\": [\n    {\n      \"DownstreamPathTemplate\": \"/api/{version}/{everything}\",\n      \"DownstreamScheme\": \"http\",\n      \"UpstreamPathTemplate\": \"/gateway/{version}/{everything}\"\n    }\n  ]\n}\n```\n\n## Bonnes pratiques\n- Utilisez le rate limiting pour protéger vos services\n- Implémentez le circuit breaker avec Polly\n- Centralisez la gestion des erreurs",
                Excerpt = "Implémentez une architecture microservices robuste avec .NET 9 et la passerelle API Ocelot.",
                CoverImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 312,
                PublishedAt = now.AddDays(-30), CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-29)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Introduction au Machine Learning avec Python", Slug = "introduction-machine-learning-python",
                Content = "Le machine learning transforme notre façon de traiter les données. Ce guide vous initie aux concepts fondamentaux avec Python.\n\n## Concepts clés\n- Apprentissage supervisé vs non supervisé\n- Régression et classification\n- Réseaux de neurones\n\n## Bibliothèques essentielles\n```python\nimport numpy as np\nimport pandas as pd\nfrom sklearn.model_selection import train_test_split\nfrom sklearn.ensemble import RandomForestClassifier\n```\n\n## Premier modèle\nNous allons entraîner un classifieur sur le jeu de données Iris et évaluer ses performances avec une matrice de confusion.",
                Excerpt = "Les fondamentaux du machine learning avec Python : algorithmes, bibliothèques et exemples pratiques.",
                CoverImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800",
                AuthorId = MemberId1, AuthorName = "Aminata Diallo", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 178,
                PublishedAt = now.AddDays(-25), CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-24)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Dockeriser votre application .NET", Slug = "dockeriser-application-dotnet",
                Content = "Docker simplifie le déploiement de vos applications .NET en les encapsulant dans des conteneurs légers et portables.\n\n## Pourquoi Docker ?\n- Environnements reproductibles\n- Déploiement simplifié\n- Scalabilité horizontale\n- Isolation des applications\n\n## Dockerfile pour .NET\n```dockerfile\nFROM mcr.microsoft.com/dotnet/sdk:9.0 AS build\nWORKDIR /src\nCOPY . .\nRUN dotnet publish -c Release -o /app\n\nFROM mcr.microsoft.com/dotnet/aspnet:9.0\nWORKDIR /app\nCOPY --from=build /app .\nENTRYPOINT [\"dotnet\", \"MonApp.dll\"]\n```\n\n## Docker Compose\nPour les applications multi-conteneurs, utilisez Docker Compose pour orchestrer vos services, bases de données et caches.",
                Excerpt = "Apprenez à conteneuriser vos applications .NET avec Docker pour un déploiement fiable.",
                CoverImageUrl = "https://images.unsplash.com/photo-1605745341112-85968b19335b?w=800",
                AuthorId = MemberId2, AuthorName = "Koffi Mensah", AuthorAvatar = "",
                PostType = "tutoriel", IsPublished = true, ViewCount = 267,
                PublishedAt = now.AddDays(-20), CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-19)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "React et TypeScript : Guide du débutant", Slug = "react-typescript-debutant",
                Content = "React combiné à TypeScript offre une expérience de développement robuste avec un typage statique.\n\n## Configuration du projet\n```bash\nnpx create-react-app mon-app --template typescript\ncd mon-app\nnpm start\n```\n\n## Composants typés\n```typescript\ninterface Props {\n  name: string;\n  age?: number;\n}\n\nconst Greeting: React.FC<Props> = ({ name, age }) => {\n  return <div>Bonjour {name}{age ? `, ${age} ans` : ''}</div>;\n};\n```\n\n## Hooks avec TypeScript\nLes hooks comme useState et useEffect s'intègrent parfaitement avec TypeScript pour un code plus sûr.",
                Excerpt = "Découvrez comment allier la puissance de React avec la sécurité de TypeScript.",
                CoverImageUrl = "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=800",
                AuthorId = MemberId3, AuthorName = "Fatoumata Barry", AuthorAvatar = "",
                PostType = "tutoriel", IsPublished = true, ViewCount = 198,
                PublishedAt = now.AddDays(-15), CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-14)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Sécuriser votre API avec IdentityServer", Slug = "securiser-api-identityserver",
                Content = "La sécurisation des API est cruciale pour toute application moderne. IdentityServer (maintenant Duende) est la solution de référence pour .NET.\n\n## OpenID Connect et OAuth 2.0\n- OpenID Connect pour l'authentification\n- OAuth 2.0 pour l'autorisation\n- Les jetons JWT pour les claims\n\n## Configuration minimale\n```csharp\nservices.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n    .AddJwtBearer(options =>\n    {\n        options.Authority = \"https://identity.exemple.com\";\n        options.Audience = \"api\";\n    });\n```\n\n## Bonnes pratiques\n- Utilisez des jetons d'accès à courte durée\n- Implémentez le refresh token\n- Validez toujours les claims côté serveur",
                Excerpt = "Guide complet pour sécuriser vos API .NET avec IdentityServer et les standards OAuth 2.0.",
                CoverImageUrl = "https://images.unsplash.com/photo-1563013544-824ae1b704d3?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 156,
                PublishedAt = now.AddDays(-12), CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-11)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "EF Core : Performance et optimisation", Slug = "ef-core-performance-optimisation",
                Content = "Entity Framework Core est un ORM puissant, mais une mauvaise utilisation peut entraîner des problèmes de performance. Voici nos conseils.\n\n## Problèmes courants\n- Problème N+1\n- Chargement paresseux non maîtrisé\n- Requêtes non filtrées\n\n## Solutions\n```csharp\n// Évitez le problème N+1\nvar blogs = await context.Blogs\n    .Include(b => b.Posts)\n    .ToListAsync();\n\n// Utilisez des projections\nvar summaries = await context.Blogs\n    .Select(b => new { b.Id, b.Title, PostCount = b.Posts.Count })\n    .ToListAsync();\n```\n\n## Indexation\nCréez des indexes sur les colonnes fréquemment utilisées dans les clauses WHERE et ORDER BY.",
                Excerpt = "Optimisez les performances de vos requêtes Entity Framework Core avec ces techniques avancées.",
                CoverImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800",
                AuthorId = MemberId4, AuthorName = "Ibrahim Sow", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 134,
                PublishedAt = now.AddDays(-10), CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-9)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "CI/CD avec GitHub Actions pour .NET", Slug = "cicd-github-actions-dotnet",
                Content = "GitHub Actions permet d'automatiser vos pipelines d'intégration et de déploiement continues pour vos projets .NET.\n\n## Workflow de base\n```yaml\nname: Build and Test\non: [push, pull_request]\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - name: Setup .NET\n        uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '9.0'\n      - run: dotnet build\n      - run: dotnet test\n```\n\n## Déploiement\nAjoutez des étapes pour déployer sur Azure, AWS ou votre propre serveur via FTP ou SSH.",
                Excerpt = "Automatisez le build, le test et le déploiement de vos applications .NET avec GitHub Actions.",
                CoverImageUrl = "https://images.unsplash.com/photo-1618401471353-b98afee0b2eb?w=800",
                AuthorId = MemberId5, AuthorName = "Aïchatou Moussa", AuthorAvatar = "",
                PostType = "tutoriel", IsPublished = true, ViewCount = 221,
                PublishedAt = now.AddDays(-7), CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-6)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "SignalR : Communication temps réel", Slug = "signalr-communication-temps-reel",
                Content = "SignalR permet d'ajouter des fonctionnalités temps réel à vos applications .NET : chat, notifications, mises à jour en direct.\n\n## Hub SignalR\n```csharp\npublic class ChatHub : Hub\n{\n    public async Task SendMessage(string user, string message)\n    {\n        await Clients.All.SendAsync(\"ReceiveMessage\", user, message);\n    }\n}\n```\n\n## Client JavaScript\n```javascript\nconst connection = new signalR.HubConnectionBuilder()\n    .withUrl(\"/chathub\")\n    .build();\n\nconnection.on(\"ReceiveMessage\", (user, message) => {\n    // Afficher le message\n});\n```\n\n## Scénarios d'utilisation\n- Tableaux de bord en direct\n- Notifications push\n- Collaboration en temps réel",
                Excerpt = "Implémentez des fonctionnalités temps réel dans vos applications .NET avec SignalR.",
                CoverImageUrl = "https://images.unsplash.com/photo-1552581234-26160f608093?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 167,
                PublishedAt = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-4)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Azure DevOps pour les développeurs .NET", Slug = "azure-devops-developpeurs-dotnet",
                Content = "Azure DevOps est une plateforme complète pour le cycle de vie des applications. Découvrez comment l'exploiter pour vos projets .NET.\n\n## Azure Boards\nGérez vos tâches avec Kanban et Scrum.\n\n## Azure Repos\nHébergez votre code avec Git.\n\n## Azure Pipelines\n```yaml\ntrigger:\n- main\n\npool:\n  vmImage: 'ubuntu-latest'\n\nsteps:\n- task: DotNetCoreCLI@2\n  inputs:\n    command: 'build'\n```\n\n## Azure Artifacts\nPubliez et partagez vos packages NuGet privés.",
                Excerpt = "Tirez parti d'Azure DevOps pour gérer, builder et déployer vos projets .NET efficacement.",
                CoverImageUrl = "https://images.unsplash.com/photo-1633356122102-3fe601e05bd2?w=800",
                AuthorId = MemberId1, AuthorName = "Aminata Diallo", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 145,
                PublishedAt = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Les design patterns en C#", Slug = "design-patterns-csharp",
                Content = "Les design patterns sont des solutions éprouvées aux problèmes récurrents en conception logicielle.\n\n## Pattern Singleton\n```csharp\npublic sealed class Singleton\n{\n    private static readonly Lazy<Singleton> _instance = new(() => new Singleton());\n    public static Singleton Instance => _instance.Value;\n    private Singleton() { }\n}\n```\n\n## Pattern Repository\nSépare la logique d'accès aux données de la logique métier.\n\n## Pattern Strategy\nPermet de changer d'algorithme à l'exécution.\n\n## Pattern Observer\nIdéal pour les systèmes d'événements et de notifications.",
                Excerpt = "Les design patterns essentiels en C# pour une architecture logicielle robuste et maintenable.",
                CoverImageUrl = "https://images.unsplash.com/photo-1516116216624-53e697fedbea?w=800",
                AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "",
                PostType = "article", IsPublished = true, ViewCount = 289,
                PublishedAt = now.AddDays(-1), CreatedAt = now.AddDays(-1), UpdatedAt = now
            }
        };
        db.Posts.AddRange(posts);

        // ---- Post Categories ----
        db.PostCategories.AddRange(
            new PostCategory { PostId = posts[0].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[1].Id, CategoryId = catMobile.Id },
            new PostCategory { PostId = posts[2].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[2].Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = posts[3].Id, CategoryId = catData.Id },
            new PostCategory { PostId = posts[4].Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = posts[5].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[6].Id, CategoryId = catSecurite.Id },
            new PostCategory { PostId = posts[7].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[8].Id, CategoryId = catDevOps.Id },
            new PostCategory { PostId = posts[9].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[10].Id, CategoryId = catDev.Id },
            new PostCategory { PostId = posts[11].Id, CategoryId = catDevOps.Id }
        );

        // ---- Post Tags ----
        db.PostTags.AddRange(
            new PostTag { PostId = posts[0].Id, TagId = tagBlazor.Id },
            new PostTag { PostId = posts[0].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[0].Id, TagId = tagCsharp.Id },
            new PostTag { PostId = posts[1].Id, TagId = tagCsharp.Id },
            new PostTag { PostId = posts[1].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[2].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[2].Id, TagId = tagApi.Id },
            new PostTag { PostId = posts[3].Id, TagId = tagPython.Id },
            new PostTag { PostId = posts[3].Id, TagId = tagMl.Id },
            new PostTag { PostId = posts[3].Id, TagId = tagIa.Id },
            new PostTag { PostId = posts[4].Id, TagId = tagDocker.Id },
            new PostTag { PostId = posts[4].Id, TagId = tagDevops.Id },
            new PostTag { PostId = posts[5].Id, TagId = tagReact.Id },
            new PostTag { PostId = posts[5].Id, TagId = tagJs.Id },
            new PostTag { PostId = posts[6].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[6].Id, TagId = tagApi.Id },
            new PostTag { PostId = posts[7].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[7].Id, TagId = tagSql.Id },
            new PostTag { PostId = posts[8].Id, TagId = tagDevops.Id },
            new PostTag { PostId = posts[8].Id, TagId = tagDocker.Id },
            new PostTag { PostId = posts[9].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[9].Id, TagId = tagCsharp.Id },
            new PostTag { PostId = posts[10].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[10].Id, TagId = tagAzure.Id },
            new PostTag { PostId = posts[10].Id, TagId = tagDevops.Id },
            new PostTag { PostId = posts[11].Id, TagId = tagCsharp.Id },
            new PostTag { PostId = posts[11].Id, TagId = tagDotnet.Id },
            new PostTag { PostId = posts[11].Id, TagId = tagTutoriel.Id }
        );

        // ---- Comments ----
        var commentList = new List<Comment>
        {
            new() { Id = Guid.NewGuid(), Content = "Excellent article ! J'ai pu créer ma première app Blazor grâce à ce guide. Merci beaucoup.", UserId = MemberId1, AuthorName = "Aminata Diallo", PostId = posts[0].Id, CreatedAt = now.AddDays(-59) },
            new() { Id = Guid.NewGuid(), Content = "Super, j'ai partagé ce tutoriel avec mon équipe. Nous allons l'utiliser pour notre prochain projet.", UserId = MemberId2, AuthorName = "Koffi Mensah", PostId = posts[0].Id, CreatedAt = now.AddDays(-58) },
            new() { Id = Guid.NewGuid(), Content = "Très bon article ! Est-ce que vous pourriez approfondir sur l'intégration avec SignalR ?", UserId = MemberId3, AuthorName = "Fatoumata Barry", PostId = posts[0].Id, CreatedAt = now.AddDays(-57) },
            new() { Id = Guid.NewGuid(), Content = "Merci pour ce guide. J'attends la suite avec impatience !", UserId = MemberId4, AuthorName = "Ibrahim Sow", PostId = posts[1].Id, CreatedAt = now.AddDays(-44) },
            new() { Id = Guid.NewGuid(), Content = "J'ai testé sur Android et iOS, les deux fonctionnent parfaitement.", UserId = MemberId5, AuthorName = "Aïchatou Moussa", PostId = posts[1].Id, CreatedAt = now.AddDays(-43) },
            new() { Id = Guid.NewGuid(), Content = "L'architecture microservices est vraiment adaptée pour notre projet actuel. Merci pour les conseils sur Ocelot.", UserId = MemberId1, AuthorName = "Aminata Diallo", PostId = posts[2].Id, CreatedAt = now.AddDays(-29) },
            new() { Id = Guid.NewGuid(), Content = "Article très intéressant. Avez-vous des ressources pour approfondir le rate limiting avec Ocelot ?", UserId = MemberId4, AuthorName = "Ibrahim Sow", PostId = posts[2].Id, CreatedAt = now.AddDays(-28) },
            new() { Id = Guid.NewGuid(), Content = "Python est vraiment le meilleur langage pour le ML. Beau tutoriel !", UserId = AdminId, AuthorName = "Admin Plateforme", PostId = posts[3].Id, CreatedAt = now.AddDays(-24) },
            new() { Id = Guid.NewGuid(), Content = "Docker a changé ma façon de développer. Excellent article introductif.", UserId = AdminId, AuthorName = "Admin Plateforme", PostId = posts[4].Id, CreatedAt = now.AddDays(-19) },
            new() { Id = Guid.NewGuid(), Content = "TypeScript + React c'est le meilleur combo pour le frontend !", UserId = MemberId5, AuthorName = "Aïchatou Moussa", PostId = posts[5].Id, CreatedAt = now.AddDays(-14) },
            new() { Id = Guid.NewGuid(), Content = "La sécurité est souvent négligée. Merci de rappeler les bonnes pratiques.", UserId = MemberId2, AuthorName = "Koffi Mensah", PostId = posts[6].Id, CreatedAt = now.AddDays(-11) },
            new() { Id = Guid.NewGuid(), Content = "Très utile ! Les problèmes N+1 m'ont déjà fait perdre des heures.", UserId = MemberId3, AuthorName = "Fatoumata Barry", PostId = posts[7].Id, CreatedAt = now.AddDays(-9) },
            new() { Id = Guid.NewGuid(), Content = "GitHub Actions est vraiment puissant. Merci pour ce guide pas à pas.", UserId = MemberId1, AuthorName = "Aminata Diallo", PostId = posts[8].Id, CreatedAt = now.AddDays(-6) },
            new() { Id = Guid.NewGuid(), Content = "SignalR c'est génial pour les notifications en temps réel. Je l'utilise pour mon dashboard.", UserId = MemberId4, AuthorName = "Ibrahim Sow", PostId = posts[9].Id, CreatedAt = now.AddDays(-4) },
            new() { Id = Guid.NewGuid(), Content = "Azure DevOps est indispensable pour nos projets d'entreprise.", UserId = MemberId2, AuthorName = "Koffi Mensah", PostId = posts[10].Id, CreatedAt = now.AddDays(-2) },
            new() { Id = Guid.NewGuid(), Content = "Les design patterns sont essentiels pour tout développeur sérieux. Bel article !", UserId = MemberId3, AuthorName = "Fatoumata Barry", PostId = posts[11].Id, CreatedAt = now.AddHours(-12) },
            new() { Id = Guid.NewGuid(), Content = "J'aurais aimé voir aussi le pattern Factory. Pouvez-vous faire un article dédié ?", UserId = MemberId5, AuthorName = "Aïchatou Moussa", PostId = posts[11].Id, CreatedAt = now.AddHours(-6) },
        };
        var replyParentId = commentList[^1].Id;
        commentList.Add(new() { Id = Guid.NewGuid(), Content = "Oui, je prévois un article complet sur les patterns de création (Factory, Builder, Prototype). Restez connecté !", UserId = AdminId, AuthorName = "Admin Plateforme", PostId = posts[11].Id, ParentCommentId = replyParentId, CreatedAt = now.AddHours(-4) });
        db.Comments.AddRange(commentList);

        // ---- Members ----
        var members = new List<Member>
        {
            new()
            {
                Id = AdminId, FullName = "Admin Plateforme", Bio = "Administrateur de la plateforme DotnetNiger. Passionné par .NET et les architectures modernes.",
                AvatarUrl = "", Country = "Niger", City = "Niamey",
                CreatedAt = now.AddDays(-90), UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = MemberId1, FullName = "Aminata Diallo", Bio = "Développeuse full-stack .NET & Angular. Contributrice open-source et organisatrice de meetups.",
                AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200", Country = "Niger", City = "Niamey",
                CreatedAt = now.AddDays(-80), UpdatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = MemberId2, FullName = "Koffi Mensah", Bio = "Architecte cloud Azure et DevOps. J'aime partager mes connaissances en CI/CD et infrastructure as code.",
                AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200", Country = "Côte d'Ivoire", City = "Abidjan",
                CreatedAt = now.AddDays(-75), UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = MemberId3, FullName = "Fatoumata Barry", Bio = "Développeuse mobile .NET MAUI et React Native. Créatrice d'applications éducatives.",
                AvatarUrl = "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=200", Country = "Guinée", City = "Conakry",
                CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = MemberId4, FullName = "Ibrahim Sow", Bio = "Data scientist et développeur Python/.NET. Spécialiste en machine learning et traitement du langage naturel.",
                AvatarUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200", Country = "Sénégal", City = "Dakar",
                CreatedAt = now.AddDays(-65), UpdatedAt = now.AddDays(-8)
            },
            new()
            {
                Id = MemberId5, FullName = "Aïchatou Moussa", Bio = "Développeuse backend .NET et passionnée de sécurité. Membre active de la communauté.",
                AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=200", Country = "Niger", City = "Niamey",
                CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-2)
            }
        };
        db.Members.AddRange(members);

        // ---- Social Links ----
        db.SocialLinks.AddRange(
            new SocialLink { MemberId = MemberId1, Platform = "GitHub", Url = "https://github.com/aminata-diallo" },
            new SocialLink { MemberId = MemberId1, Platform = "LinkedIn", Url = "https://linkedin.com/in/aminata-diallo" },
            new SocialLink { MemberId = MemberId2, Platform = "GitHub", Url = "https://github.com/koffi-mensah" },
            new SocialLink { MemberId = MemberId2, Platform = "Twitter", Url = "https://twitter.com/koffimensah" },
            new SocialLink { MemberId = MemberId3, Platform = "LinkedIn", Url = "https://linkedin.com/in/fatou-barry" },
            new SocialLink { MemberId = MemberId4, Platform = "GitHub", Url = "https://github.com/ibrahim-sow" },
            new SocialLink { MemberId = MemberId5, Platform = "Twitter", Url = "https://twitter.com/achatou_m" }
        );

        // ---- Events ----
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Meetup .NET 9 : Les nouveautés", Slug = "meetup-dotnet9-nouveautes",
                Description = "Venez découvrir les nouvelles fonctionnalités de .NET 9 : améliorations des performances, nouvelles API et évolutions du langage C#. Au programme : démos en direct, retours d'expérience et questions-réponses.",
                Location = "Niamey, Niger - Espace Numérique", EventType = "meetup", Category = "Développement Web",
                StartDate = now.AddDays(-20), EndDate = now.AddDays(-20).AddHours(3),
                CreatedBy = AdminId, OrganizerName = "Admin Plateforme", Capacity = 50, RegisteredCount = 35,
                IsPublished = true, PublishedAt = now.AddDays(-30),
                CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-20),
                CoverImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800"
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Workshop Blazor Avancé", Slug = "workshop-blazor-avance",
                Description = "Workshop pratique de 4 heures sur Blazor : composants avancés, State management, SignalR et déploiement. Apportez votre ordinateur avec Visual Studio 2022 ou VS Code et le SDK .NET 9 installé.",
                Location = "En ligne - Google Meet", EventType = "workshop", Category = "Développement Web",
                StartDate = now.AddDays(-10), EndDate = now.AddDays(-10).AddHours(4),
                CreatedBy = AdminId, OrganizerName = "Admin Plateforme", Capacity = 30, RegisteredCount = 28,
                IsPublished = true, PublishedAt = now.AddDays(-25),
                CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-10),
                CoverImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=800",
                MeetupLink = "https://meet.google.com/abc-defg-hij"
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Conférence : IA Générative avec .NET", Slug = "conference-ia-generative-dotnet",
                Description = "Découvrez comment intégrer l'IA générative dans vos applications .NET : OpenAI, Azure OpenAI, Semantic Kernel et ML.NET. Une conférence animée par des experts du domaine.",
                Location = "Niamey, Niger - Université Abdou Moumouni", EventType = "conference", Category = "Data & IA",
                StartDate = now.AddDays(5), EndDate = now.AddDays(5).AddHours(6),
                CreatedBy = AdminId, OrganizerName = "Admin Plateforme", Capacity = 100, RegisteredCount = 45,
                IsPublished = true, PublishedAt = now.AddDays(-15),
                CreatedAt = now.AddDays(-15), UpdatedAt = now,
                CoverImageUrl = "https://images.unsplash.com/photo-1485827404703-89b55fcc595e?w=800"
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Hackathon : Smart City Niger", Slug = "hackathon-smart-city-niger",
                Description = "48h pour créer des solutions innovantes pour les villes intelligentes au Niger. Thèmes : mobilité, énergie, santé et éducation. Équipes de 3 à 5 personnes. Prix à gagner : 1 000 000 FCFA.",
                Location = "Niamey, Niger - Centre Incubateur", EventType = "hackathon", Category = "Communauté",
                StartDate = now.AddDays(15), EndDate = now.AddDays(17),
                CreatedBy = AdminId, OrganizerName = "Admin Plateforme", Capacity = 60, RegisteredCount = 32,
                IsPublished = true, PublishedAt = now.AddDays(-10),
                CreatedAt = now.AddDays(-10), UpdatedAt = now,
                CoverImageUrl = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?w=800"
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Formation Docker & Kubernetes", Slug = "formation-docker-kubernetes",
                Description = "Formation intensive de 2 jours sur Docker et Kubernetes pour les développeurs .NET. De la création de containers à l'orchestration en production.",
                Location = "En ligne - Zoom", EventType = "workshop", Category = "DevOps",
                StartDate = now.AddDays(25), EndDate = now.AddDays(26),
                CreatedBy = MemberId2, OrganizerName = "Koffi Mensah", Capacity = 25, RegisteredCount = 18,
                IsPublished = true, PublishedAt = now.AddDays(-5),
                CreatedAt = now.AddDays(-5), UpdatedAt = now,
                CoverImageUrl = "https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=800"
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Webinaire : Cybersécurité pour développeurs", Slug = "webinaire-cybersecurite-developpeurs",
                Description = "Les bonnes pratiques de sécurité à adopter dès maintenant : OWASP Top 10, sécurisation des API, gestion des secrets, et prévention des attaques courantes.",
                Location = "En ligne - Microsoft Teams", EventType = "webinar", Category = "Sécurité",
                StartDate = now.AddDays(40), EndDate = now.AddDays(40).AddHours(2),
                CreatedBy = MemberId5, OrganizerName = "Aïchatou Moussa", Capacity = 100, RegisteredCount = 12,
                IsPublished = true, PublishedAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-1), UpdatedAt = now,
                CoverImageUrl = "https://images.unsplash.com/photo-1555949963-ff9fe0c870eb?w=800"
            }
        };
        db.Events.AddRange(events);

        // ---- Event Registrations ----
        db.EventRegistrations.AddRange(
            new EventRegistration { EventId = events[0].Id, UserId = MemberId1, UserName = "Aminata Diallo", RegisteredAt = now.AddDays(-20), RegistrationStatus = "confirmed", IsAttended = true },
            new EventRegistration { EventId = events[0].Id, UserId = MemberId2, UserName = "Koffi Mensah", RegisteredAt = now.AddDays(-19), RegistrationStatus = "confirmed", IsAttended = true },
            new EventRegistration { EventId = events[0].Id, UserId = MemberId3, UserName = "Fatoumata Barry", RegisteredAt = now.AddDays(-18), RegistrationStatus = "confirmed", IsAttended = false },
            new EventRegistration { EventId = events[1].Id, UserId = MemberId4, UserName = "Ibrahim Sow", RegisteredAt = now.AddDays(-12), RegistrationStatus = "confirmed", IsAttended = true },
            new EventRegistration { EventId = events[1].Id, UserId = MemberId5, UserName = "Aïchatou Moussa", RegisteredAt = now.AddDays(-11), RegistrationStatus = "confirmed", IsAttended = true },
            new EventRegistration { EventId = events[2].Id, UserId = MemberId1, UserName = "Aminata Diallo", RegisteredAt = now.AddDays(-10), RegistrationStatus = "confirmed", IsAttended = false },
            new EventRegistration { EventId = events[2].Id, UserId = MemberId2, UserName = "Koffi Mensah", RegisteredAt = now.AddDays(-9), RegistrationStatus = "confirmed", IsAttended = false },
            new EventRegistration { EventId = events[3].Id, UserId = MemberId3, UserName = "Fatoumata Barry", RegisteredAt = now.AddDays(-8), RegistrationStatus = "pending", IsAttended = false },
            new EventRegistration { EventId = events[3].Id, UserId = MemberId4, UserName = "Ibrahim Sow", RegisteredAt = now.AddDays(-7), RegistrationStatus = "confirmed", IsAttended = false },
            new EventRegistration { EventId = events[4].Id, UserId = AdminId, UserName = "Admin Plateforme", RegisteredAt = now.AddDays(-4), RegistrationStatus = "confirmed", IsAttended = false }
        );

        // ---- Event Tags ----
        db.EventTags.AddRange(
            new EventTag { EventId = events[0].Id, TagId = tagDotnet.Id },
            new EventTag { EventId = events[0].Id, TagId = tagCsharp.Id },
            new EventTag { EventId = events[1].Id, TagId = tagBlazor.Id },
            new EventTag { EventId = events[1].Id, TagId = tagDotnet.Id },
            new EventTag { EventId = events[2].Id, TagId = tagIa.Id },
            new EventTag { EventId = events[2].Id, TagId = tagMl.Id },
            new EventTag { EventId = events[2].Id, TagId = tagPython.Id },
            new EventTag { EventId = events[3].Id, TagId = tagOpensource.Id },
            new EventTag { EventId = events[4].Id, TagId = tagDocker.Id },
            new EventTag { EventId = events[4].Id, TagId = tagDevops.Id }
        );

        // ---- Speakers ----
        db.Speakers.AddRange(
            new Speaker { EventId = events[2].Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Conférencier principal" },
            new Speaker { EventId = events[2].Id, UserId = MemberId1, Name = "Aminata Diallo", Role = "Intervenante" },
            new Speaker { EventId = events[4].Id, UserId = MemberId2, Name = "Koffi Mensah", Role = "Formateur" },
            new Speaker { EventId = events[5].Id, UserId = MemberId5, Name = "Aïchatou Moussa", Role = "Intervenante" }
        );

        // ---- Resources ----
        var resources = new List<Resource>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Cheatsheet C# 12", Slug = "cheatsheet-csharp-12",
                Description = "Un aide-mémoire complet pour C# 12 : nouvelles fonctionnalités, syntaxe, patterns et bonnes pratiques.",
                Url = "https://cheatsheet.example.com/csharp-12", ResourceType = "document", Level = "débutant",
                CreatedBy = AdminId, ViewCount = 450, CreatedAt = now.AddDays(-90), UpdatedAt = now.AddDays(-90)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Template Blazor Clean Architecture", Slug = "template-blazor-clean-architecture",
                Description = "Un template de projet Blazor WebAssembly avec Clean Architecture, authentification, et gestion des erreurs prête à l'emploi.",
                Url = "https://github.com/dotnetniger/blazor-clean-template", ResourceType = "template", Level = "intermédiaire",
                CreatedBy = AdminId, ViewCount = 320, CreatedAt = now.AddDays(-80), UpdatedAt = now.AddDays(-80)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Cours complet Entity Framework Core", Slug = "cours-complet-entity-framework-core",
                Description = "Formation vidéo de 10 heures couvrant EF Core : DbContext, migrations, requêtes, performances et déploiement.",
                Url = "https://learn.example.com/ef-core", ResourceType = "video", Level = "débutant",
                CreatedBy = MemberId4, ViewCount = 280, CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-70)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Guide Azure pour développeurs .NET", Slug = "guide-azure-developpeurs-dotnet",
                Description = "Ebook gratuit de 150 pages sur l'utilisation d'Azure pour vos applications .NET : App Services, Functions, Cosmos DB et plus.",
                Url = "https://azure.guide/dotnet", ResourceType = "ebook", Level = "intermédiaire",
                CreatedBy = MemberId2, ViewCount = 195, CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-60)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Projet Open Source : Gestionnaire de tâches", Slug = "projet-open-source-gestionnaire-taches",
                Description = "Un gestionnaire de tâches collaboratif en .NET MAUI avec synchronisation cloud et notifications push. Contribuez !",
                Url = "https://github.com/dotnetniger/task-manager", ResourceType = "code", Level = "avancé",
                CreatedBy = MemberId1, ViewCount = 150, CreatedAt = now.AddDays(-50), UpdatedAt = now.AddDays(-50)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Workshop Docker Avancé", Slug = "workshop-docker-avance",
                Description = "Diapositives et exemples du workshop Docker avancé : multi-stage builds, Docker Compose, orchestration avec Swarm et Kubernetes.",
                Url = "https://slides.example.com/docker-avance", ResourceType = "presentation", Level = "avancé",
                CreatedBy = MemberId2, ViewCount = 210, CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-40)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "API RESTful avec .NET 9", Slug = "api-restful-dotnet-9",
                Description = "Guide complet pour créer des API RESTful avec .NET 9 : Minimal APIs, versioning, documentation Swagger et tests d'intégration.",
                Url = "https://api.guide/dotnet9", ResourceType = "document", Level = "intermédiaire",
                CreatedBy = AdminId, ViewCount = 380, CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-30)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Podcast : L'écosystème .NET en Afrique", Slug = "podcast-ecosysteme-dotnet-afrique",
                Description = "Podcast mensuel sur l'actualité .NET en Afrique : interviews, projets, événements et opportunités pour les développeurs.",
                Url = "https://podcast.example.com/dotnet-afrique", ResourceType = "video", Level = "débutant",
                CreatedBy = MemberId3, ViewCount = 520, CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-20)
            }
        };
        db.Resources.AddRange(resources);

        // ---- Resource Categories ----
        db.ResourceCategories.AddRange(
            new ResourceCategory { ResourceId = resources[0].Id, CategoryId = catDev.Id },
            new ResourceCategory { ResourceId = resources[1].Id, CategoryId = catDev.Id },
            new ResourceCategory { ResourceId = resources[2].Id, CategoryId = catDev.Id },
            new ResourceCategory { ResourceId = resources[3].Id, CategoryId = catDevOps.Id },
            new ResourceCategory { ResourceId = resources[4].Id, CategoryId = catMobile.Id },
            new ResourceCategory { ResourceId = resources[5].Id, CategoryId = catDevOps.Id },
            new ResourceCategory { ResourceId = resources[6].Id, CategoryId = catDev.Id },
            new ResourceCategory { ResourceId = resources[7].Id, CategoryId = catCommunaute.Id }
        );

        // ---- Resource Tags ----
        db.ResourceTags.AddRange(
            new ResourceTag { ResourceId = resources[0].Id, TagId = tagCsharp.Id },
            new ResourceTag { ResourceId = resources[0].Id, TagId = tagDotnet.Id },
            new ResourceTag { ResourceId = resources[1].Id, TagId = tagBlazor.Id },
            new ResourceTag { ResourceId = resources[1].Id, TagId = tagDotnet.Id },
            new ResourceTag { ResourceId = resources[2].Id, TagId = tagDotnet.Id },
            new ResourceTag { ResourceId = resources[2].Id, TagId = tagSql.Id },
            new ResourceTag { ResourceId = resources[3].Id, TagId = tagAzure.Id },
            new ResourceTag { ResourceId = resources[4].Id, TagId = tagDotnet.Id },
            new ResourceTag { ResourceId = resources[5].Id, TagId = tagDocker.Id },
            new ResourceTag { ResourceId = resources[5].Id, TagId = tagDevops.Id },
            new ResourceTag { ResourceId = resources[6].Id, TagId = tagDotnet.Id },
            new ResourceTag { ResourceId = resources[6].Id, TagId = tagApi.Id },
            new ResourceTag { ResourceId = resources[7].Id, TagId = tagDotnet.Id }
        );

        // ---- Projects ----
        db.Projects.AddRange(
            new Project
            {
                Id = Guid.NewGuid(), Title = "Plateforme E-commerce DotnetNiger", Slug = "plateforme-e-commerce-dotnetniger",
                Description = "Une plateforme e-commerce complète bâtie avec Blazor et .NET 9, intégrant des paiements Mobile Money et une gestion d'inventaire temps réel.",
                Url = "https://github.com/dotnetniger/ecommerce", GithubUrl = "https://github.com/dotnetniger/ecommerce",
                Technologies = "Blazor,.NET 9,Entity Framework,PostgreSQL",
                Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme",
                IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-90)
            },
            new Project
            {
                Id = Guid.NewGuid(), Title = "API Gateway DotnetNiger", Slug = "api-gateway-dotnetniger",
                Description = "Passerelle API centralisée avec Ocelot, rate limiting, cache Swagger et monitoring des performances par endpoint.",
                Url = "https://github.com/dotnetniger/gateway", GithubUrl = "https://github.com/dotnetniger/gateway",
                Technologies = "Ocelot,.NET 9,Swagger,Prometheus",
                Status = "active", CreatedBy = AdminId, AuthorName = "Admin Plateforme",
                IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-80)
            },
            new Project
            {
                Id = Guid.NewGuid(), Title = "Application Mobile Meetup", Slug = "application-mobile-meetup",
                Description = "Application mobile .NET MAUI pour la gestion des meetups DotnetNiger avec notifications push et QR code pour les inscriptions.",
                Url = "https://github.com/dotnetniger/meetup-app", GithubUrl = "https://github.com/dotnetniger/meetup-app",
                Technologies = ".NET MAUI,C#,SignalR,Azure",
                Status = "active", CreatedBy = MemberId1, AuthorName = "Aminata Diallo",
                IsFeatured = false, IsPublished = true, CreatedAt = now.AddDays(-70)
            },
            new Project
            {
                Id = Guid.NewGuid(), Title = "Plateforme de Mentorat", Slug = "plateforme-de-mentorat",
                Description = "Plateforme connectant mentors et mentorés dans le domaine tech au Niger. Matching intelligent basé sur les compétences et objectifs.",
                Url = "https://github.com/dotnetniger/mentorship", GithubUrl = "https://github.com/dotnetniger/mentorship",
                Technologies = "Blazor,.NET 9,Azure SQL,SignalR",
                Status = "active", CreatedBy = MemberId2, AuthorName = "Koffi Mensah",
                IsFeatured = true, IsPublished = true, CreatedAt = now.AddDays(-60)
            },
            new Project
            {
                Id = Guid.NewGuid(), Title = "Outil de Gestion des Événements", Slug = "outil-gestion-evenements",
                Description = "SaaS de gestion d'événements avec inscriptions, check-in par QR code, sondages en direct et analytics.",
                Url = "https://github.com/dotnetniger/event-manager", GithubUrl = "https://github.com/dotnetniger/event-manager",
                Technologies = "Blazor,.NET 9,Redis,PostgreSQL",
                Status = "beta", CreatedBy = MemberId3, AuthorName = "Fatoumata Barry",
                IsFeatured = false, IsPublished = true, CreatedAt = now.AddDays(-50)
            }
        );

        // ---- Partners ----
        db.Partners.AddRange(
            new Partner { Id = Guid.NewGuid(), Name = "Microsoft Niger", Slug = "microsoft-niger", Description = "Partenaire technologique officiel, supportant la communauté avec des ressources Azure et des formations.", LogoUrl = "https://img.icons8.com/color/96/microsoft.png", WebsiteUrl = "https://www.microsoft.com", PartnerType = "sponsor", SortOrder = 1, IsActive = true, CreatedAt = now.AddDays(-60) },
            new Partner { Id = Guid.NewGuid(), Name = "Orange Niger", Slug = "orange-niger", Description = "Opérateur télécom partenaire, facilitant l'organisation des événements et meetups.", LogoUrl = "https://img.icons8.com/color/96/orange.png", WebsiteUrl = "https://www.orange.ne", PartnerType = "sponsor", SortOrder = 2, IsActive = true, CreatedAt = now.AddDays(-50) },
            new Partner { Id = Guid.NewGuid(), Name = "Sonatel", Slug = "sonatel", Description = "Fournisseur d'accès internet et services cloud pour la communauté.", LogoUrl = "https://img.icons8.com/color/96/internet.png", WebsiteUrl = "https://www.sonatel.sn", PartnerType = "partner", SortOrder = 3, IsActive = true, CreatedAt = now.AddDays(-40) },
            new Partner { Id = Guid.NewGuid(), Name = "IN Web Services", Slug = "in-web-services", Description = "Agence web spécialisée dans le développement d'applications .NET et Angular.", LogoUrl = "https://img.icons8.com/color/96/cloud.png", WebsiteUrl = "https://www.inweb.ne", PartnerType = "partner", SortOrder = 4, IsActive = true, CreatedAt = now.AddDays(-30) },
            new Partner { Id = Guid.NewGuid(), Name = "Université Abdou Moumouni", Slug = "universite-abdou-moumouni", Description = "Partenaire académique pour la formation et la recherche en informatique.", LogoUrl = "https://img.icons8.com/color/96/university.png", WebsiteUrl = "https://www.uam.ne", PartnerType = "educational", SortOrder = 5, IsActive = true, CreatedAt = now.AddDays(-20) },
            new Partner { Id = Guid.NewGuid(), Name = "L'Inculab Niger", Slug = "incubab-niger", Description = "Incubateur de startups tech accompagnant les jeunes entrepreneurs numériques.", LogoUrl = "https://img.icons8.com/color/96/idea.png", WebsiteUrl = "https://www.incubab.ne", PartnerType = "incubator", SortOrder = 6, IsActive = true, CreatedAt = now.AddDays(-10) }
        );

        // ---- Notifications ----
        db.Notifications.AddRange(
            new Notification { UserId = AdminId, Message = "Bienvenue sur la plateforme DotnetNiger !", IsRead = true, CreatedAt = now.AddDays(-90) },
            new Notification { UserId = AdminId, Message = "Votre article 'Introduction à Blazor' a été publié avec succès.", IsRead = true, CreatedAt = now.AddDays(-60) },
            new Notification { UserId = AdminId, Message = "Nouvel inscrit au meetup .NET 9 : Aminata Diallo", IsRead = true, CreatedAt = now.AddDays(-20) },
            new Notification { UserId = AdminId, Message = "Un nouveau commentaire sur votre article 'Design Patterns'", IsRead = false, CreatedAt = now.AddHours(-12) },
            new Notification { UserId = AdminId, Message = "Votre événement 'Conférence IA' approche (J-5)", IsRead = false, CreatedAt = now.AddHours(-2) },
            new Notification { UserId = MemberId1, Message = "Bienvenue sur la plateforme DotnetNiger !", IsRead = true, CreatedAt = now.AddDays(-80) },
            new Notification { UserId = MemberId1, Message = "Votre article 'Machine Learning' a été publié.", IsRead = true, CreatedAt = now.AddDays(-25) },
            new Notification { UserId = MemberId1, Message = "3 nouvelles personnes se sont inscrites au hackathon.", IsRead = false, CreatedAt = now.AddHours(-48) },
            new Notification { UserId = MemberId2, Message = "Votre formation Docker a été approuvée.", IsRead = true, CreatedAt = now.AddDays(-5) }
        );

        // ---- Contact Messages ----
        db.ContactMessages.AddRange(
            new ContactMessage { FullName = "Moussa Ibrahim", Email = "moussa@example.com", Subject = "Demande de partenariat", Message = "Bonjour, je représente l'entreprise TechNiger et nous souhaiterions devenir partenaires de votre communauté pour organiser des événements ensemble.", CreatedAt = now.AddDays(-15), IsRead = true },
            new ContactMessage { FullName = "Aïssa Maïga", Email = "aissa@example.com", Subject = "Proposition de conférence", Message = "Je suis développeuse .NET senior et je souhaiterais proposer une conférence sur l'architecture microservices lors de votre prochain meetup.", CreatedAt = now.AddDays(-5), IsRead = false },
            new ContactMessage { FullName = "Amadou Diallo", Email = "amadou@example.com", Subject = "Question sur l'adhésion", Message = "Bonjour, comment puis-je devenir membre actif de la communauté ? Y a-t-il des conditions particulières ? Merci !", CreatedAt = now.AddHours(-48), IsRead = false }
        );

        // ---- Newsletter Subscriptions ----
        db.NewsletterSubscriptions.AddRange(
            new NewsletterSubscription
            {
                Id = Guid.NewGuid(), Email = "demo@dotnetniger.com", Name = "Membre Demo",
                IsActive = true, UnsubscribeToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
                SubscribedAt = now.AddDays(-90)
            },
            new NewsletterSubscription
            {
                Id = Guid.NewGuid(), Email = "admin@dotnetniger.com", Name = "Admin Plateforme",
                IsActive = true, UnsubscribeToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
                SubscribedAt = now.AddDays(-80)
            },
            new NewsletterSubscription
            {
                Id = Guid.NewGuid(), Email = "contact@dotnetniger.com", Name = "Contact",
                IsActive = true, UnsubscribeToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
                SubscribedAt = now.AddDays(-70)
            }
        );

        // ---- Certificates ----
        db.Certificates.AddRange(
            new Certificate { UserId = MemberId1, CertificateUrl = "https://certs.example.com/aminata-dotnet", CertificateType = "workshop", Status = "Approved", SubmissionDate = now.AddDays(-60), ReviewedAt = now.AddDays(-55) },
            new Certificate { UserId = MemberId2, CertificateUrl = "https://certs.example.com/koffi-azure", CertificateType = "certification", Status = "Approved", SubmissionDate = now.AddDays(-50), ReviewedAt = now.AddDays(-45) },
            new Certificate { UserId = MemberId3, CertificateUrl = "https://certs.example.com/fatou-maui", CertificateType = "workshop", Status = "Pending", SubmissionDate = now.AddDays(-10) }
        );

        await db.SaveChangesAsync();
    }
}
