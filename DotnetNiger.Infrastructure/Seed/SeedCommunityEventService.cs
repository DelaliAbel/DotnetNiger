using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Seed;

public static class SeedCommunityEventService
{
    static readonly Guid AdminId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    public static void Seed(DotnetNigerDbContext db, DateTime now, List<Tag> tags, List<Category> cats)
    {
        var t = (string n) => tags.First(x => x.Name == n);
        var events = SeedEventData(db, now);
        SeedEventMedia(db, events);
        SeedEventSpeakers(db, events);
        SeedEventRegistrations(db, now, events);
        SeedEventTags(db, events, tags);
        SeedEventComments(db, now, events);
    }

    static List<Event> SeedEventData(DotnetNigerDbContext db, DateTime now)
    {
        var events = new List<Event>
        {
            new() { Id = Guid.NewGuid(), Title = "Meetup .NET 9 : Les nouveautés", Slug = "meetup-dotnet9-nouveautes", Description = "Venez découvrir les nouveautés de .NET 9 : performances, nouvelles API et évolutions du langage C#. Au programme : démos en direct, retours d'expérience et Q&A.", Location = "Niamey, Niger", EventType = "meetup", Category = "Développement Web", Status = EventStatus.Published, StartDate = now.AddDays(-20), EndDate = now.AddDays(-20).AddHours(3), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 50, RegisteredCount = 42, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-30), CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-20), CoverImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Workshop Blazor Avancé", Slug = "workshop-blazor-avance", Description = "Workshop pratique de 4h : composants avancés, State management (Fluxor), SignalR temps réel et déploiement sur Azure Static Web Apps.", Location = "En ligne", EventType = "workshop", Category = "Développement Web", Status = EventStatus.Published, StartDate = now.AddDays(-10), EndDate = now.AddDays(-10).AddHours(4), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 30, RegisteredCount = 28, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-25), CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-10), CoverImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=800", MeetupLink = "https://meet.google.com/abc-defg-hij" },
            new() { Id = Guid.NewGuid(), Title = "Conférence IA Générative avec .NET", Slug = "conference-ia-generative-dotnet", Description = "Conférence complète sur l'intégration de l'IA générative dans vos apps .NET : OpenAI, Azure OpenAI, Semantic Kernel, ML.NET et bonnes pratiques de prompt engineering.", Location = "Niamey, Niger", EventType = "conference", Category = "Data & IA", Status = EventStatus.Published, StartDate = now.AddDays(5), EndDate = now.AddDays(5).AddHours(6), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 100, RegisteredCount = 58, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-15), CreatedAt = now.AddDays(-15), UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1485827404703-89b55fcc595e?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Hackathon Smart City Niger", Slug = "hackathon-smart-city-niger", Description = "48h pour créer des solutions innovantes pour les villes intelligentes au Niger. Thèmes : mobilité, énergie, eau, santé et éducation. Lots à gagner pour les 3 meilleures équipes.", Location = "Niamey, Niger", EventType = "hackathon", Category = "Communauté", Status = EventStatus.Published, StartDate = now.AddDays(15), EndDate = now.AddDays(17), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 60, RegisteredCount = 37, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-10), CreatedAt = now.AddDays(-10), UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Formation Docker & Kubernetes", Slug = "formation-docker-kubernetes", Description = "Formation intensive de 2 jours sur Docker et Kubernetes pour les développeurs .NET. TP pratiques : multi-stage builds, Docker Compose, déploiement sur AKS.", Location = "En ligne", EventType = "workshop", Category = "DevOps", Status = EventStatus.Published, StartDate = now.AddDays(25), EndDate = now.AddDays(26), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 25, RegisteredCount = 22, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Webinaire Cybersécurité pour développeurs", Slug = "webinaire-cybersecurite-developpeurs", Description = "Bonnes pratiques de sécurité : OWASP Top 10, sécurisation des API REST, gestion des secrets avec Azure Key Vault, authentication JWT et refresh tokens.", Location = "En ligne", EventType = "webinar", Category = "Sécurité", Status = EventStatus.Published, StartDate = now.AddDays(40), EndDate = now.AddDays(40).AddHours(2), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 100, RegisteredCount = 19, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(-1), CreatedAt = now.AddDays(-1), UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1555949963-ff9fe0c870eb?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Meetup Débutant C# : Les bases solides", Slug = "meetup-debutant-csharp-bases", Description = "Soirée dédiée aux débutants en C# : variables, types, classes, héritage, interfaces, LINQ et async/await. Apportez votre ordinateur pour coder avec nous !", Location = "Niamey, Niger", EventType = "meetup", Category = "Développement Web", Status = EventStatus.Published, StartDate = now.AddDays(50), EndDate = now.AddDays(50).AddHours(2), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 40, RegisteredCount = 15, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(0), CreatedAt = now, UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1531482615713-2afd69097998?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Conférence Cloud Azure pour .NET", Slug = "conference-cloud-azure-dotnet", Description = "Découvrez comment tirer parti d'Azure pour vos applications .NET : App Services, Functions, Cosmos DB, Service Bus et monitoring avec Application Insights.", Location = "Niamey, Niger", EventType = "conference", Category = "Cloud", Status = EventStatus.Published, StartDate = now.AddDays(60), EndDate = now.AddDays(60).AddHours(5), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 80, RegisteredCount = 31, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(1), CreatedAt = now, UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1560264280-88b68371db39?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Workshop API RESTful avec .NET 9", Slug = "workshop-api-restful-dotnet9", Description = "Atelier pratique pour construire une API RESTful complète avec .NET 9 : Minimal APIs, validation FluentValidation, versioning, Swagger/OpenAPI et tests d'intégration.", Location = "En ligne", EventType = "workshop", Category = "Développement Web", Status = EventStatus.Published, StartDate = now.AddDays(75), EndDate = now.AddDays(75).AddHours(3), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 35, RegisteredCount = 12, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(2), CreatedAt = now, UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Hackathon Open Source Weekend", Slug = "hackathon-open-source-weekend", Description = "Un week-end pour contribuer à des projets open source .NET. Encadrement par des mainteneurs, découverte de Git et GitHub, et soumission de vos premières PR.", Location = "En ligne", EventType = "hackathon", Category = "Open Source", Status = EventStatus.Published, StartDate = now.AddDays(90), EndDate = now.AddDays(92), CreatedBy = AdminId, OrganizerId = AdminId, OrganizerName = "Admin Plateforme", Capacity = 50, RegisteredCount = 8, IsPublished = true, IsArchived = false, IsDeleted = false, PublishedAt = now.AddDays(3), CreatedAt = now, UpdatedAt = now, CoverImageUrl = "https://images.unsplash.com/photo-1526925539333-8b91bafd4b02?w=800" },
        };
        db.Events.AddRange(events);
        return events;
    }

static void SeedEventMedia(DotnetNigerDbContext db, List<Event> events)
    {
        var ev = (int i) => events[i];
        db.EventMedias.AddRange(
            new EventMedia { Id = Guid.NewGuid(), EventId = ev(2).Id, Type = "image", FileUrl = "https://images.unsplash.com/photo-1485827404703-89b55fcc595e?w=800", Title = "Conférence IA - Photo 1" },
            new EventMedia { Id = Guid.NewGuid(), EventId = ev(2).Id, Type = "image", FileUrl = "https://images.unsplash.com/photo-1489875347897-49f64b51c1f8?w=800", Title = "Conférence IA - Photo 2" },
            new EventMedia { Id = Guid.NewGuid(), EventId = ev(3).Id, Type = "image", FileUrl = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?w=800", Title = "Hackathon Smart City" },
            new EventMedia { Id = Guid.NewGuid(), EventId = ev(7).Id, Type = "image", FileUrl = "https://images.unsplash.com/photo-1560264280-88b68371db39?w=800", Title = "Conférence Azure" }
        );
    }

    static void SeedEventSpeakers(DotnetNigerDbContext db, List<Event> events)
    {
        var ev = (int i) => events[i];
        db.Speakers.AddRange(
            new Speaker { Id = Guid.NewGuid(), EventId = ev(2).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Conférencier principal", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(2).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Intervenant - Semantic Kernel", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(2).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Intervenant - ML.NET", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(4).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Formateur principal", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(5).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Intervenant", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(6).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Animateur", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(7).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Conférencier principal", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(7).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Intervenant - App Services", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(8).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Formateur", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" },
            new Speaker { Id = Guid.NewGuid(), EventId = ev(9).Id, UserId = AdminId, Name = "Admin Plateforme", Role = "Mentor principal", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff" }
        );
    }

    static void SeedEventRegistrations(DotnetNigerDbContext db, DateTime now, List<Event> events)
    {
        var ev = (int i) => events[i];
        db.EventRegistrations.AddRange(
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(0).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(-20), RegistrationStatus = "confirmed" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(1).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(-12), RegistrationStatus = "confirmed" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(2).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(-10), RegistrationStatus = "confirmed" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(3).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(-8), RegistrationStatus = "pending" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(4).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(-4), RegistrationStatus = "confirmed" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(7).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(0), RegistrationStatus = "confirmed" },
            new EventRegistration { Id = Guid.NewGuid(), EventId = ev(8).Id, UserId = AdminId, UserName = "Admin Plateforme", AvatarUrl = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", RegisteredAt = now.AddDays(1), RegistrationStatus = "confirmed" }
        );
    }

    static void SeedEventTags(DotnetNigerDbContext db, List<Event> events, List<Tag> tags)
    {
        var t = (string n) => tags.First(x => x.Name == n);
        var ev = (int i) => events[i];
        db.EventTags.AddRange(
            new EventTag { EventId = ev(0).Id, TagId = t("dotnet").Id }, new EventTag { EventId = ev(0).Id, TagId = t("csharp").Id },
            new EventTag { EventId = ev(1).Id, TagId = t("blazor").Id }, new EventTag { EventId = ev(1).Id, TagId = t("dotnet").Id },
            new EventTag { EventId = ev(2).Id, TagId = t("ia").Id }, new EventTag { EventId = ev(2).Id, TagId = t("machine-learning").Id }, new EventTag { EventId = ev(2).Id, TagId = t("dotnet").Id },
            new EventTag { EventId = ev(3).Id, TagId = t("open-source").Id },
            new EventTag { EventId = ev(4).Id, TagId = t("docker").Id }, new EventTag { EventId = ev(4).Id, TagId = t("kubernetes").Id }, new EventTag { EventId = ev(4).Id, TagId = t("devops").Id },
            new EventTag { EventId = ev(5).Id, TagId = t("security").Id }, new EventTag { EventId = ev(5).Id, TagId = t("api").Id },
            new EventTag { EventId = ev(6).Id, TagId = t("csharp").Id }, new EventTag { EventId = ev(6).Id, TagId = t("dotnet").Id },
            new EventTag { EventId = ev(7).Id, TagId = t("azure").Id }, new EventTag { EventId = ev(7).Id, TagId = t("dotnet").Id },
            new EventTag { EventId = ev(8).Id, TagId = t("dotnet").Id }, new EventTag { EventId = ev(8).Id, TagId = t("api").Id },
            new EventTag { EventId = ev(9).Id, TagId = t("open-source").Id }, new EventTag { EventId = ev(9).Id, TagId = t("git").Id }
        );
    }

    static void SeedEventComments(DotnetNigerDbContext db, DateTime now, List<Event> events)
    {
        var ev = (int i) => events[i];
        db.Comments.AddRange(
            new Comment { Id = Guid.NewGuid(), Content = "Super conférence ! Le passage sur Semantic Kernel était très intéressant.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", EventId = ev(2).Id, CreatedAt = now.AddDays(-3) },
            new Comment { Id = Guid.NewGuid(), Content = "Hâte de participer au hackathon ! J'ai déjà une idée de projet.", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", EventId = ev(3).Id, CreatedAt = now.AddDays(-2) },
            new Comment { Id = Guid.NewGuid(), Content = "Est-ce que la formation sera disponible en replay ?", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", EventId = ev(4).Id, CreatedAt = now.AddDays(-1) },
            new Comment { Id = Guid.NewGuid(), Content = "Le meetup débutant C# tombe au bon moment pour mon équipe !", UserId = AdminId, AuthorId = AdminId, AuthorName = "Admin Plateforme", AuthorAvatar = "https://ui-avatars.com/api/?name=Admin+Plateforme&background=512BD4&color=fff", EventId = ev(6).Id, CreatedAt = now }
        );
    }
}