namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Statistiques du tableau de bord administrateur.</summary>
public record DashboardStats(
    // <summary>Nombre total d'articles.</summary>
    int PostsCount,
    // <summary>Nombre d'articles publiés.</summary>
    int PublishedPostsCount,
    // <summary>Nombre d'articles en brouillon.</summary>
    int DraftPostsCount,
    // <summary>Nombre total d'événements.</summary>
    int EventsCount,
    // <summary>Nombre d'événements à venir.</summary>
    int UpcomingEventsCount,
    // <summary>Nombre d'événements passés.</summary>
    int PastEventsCount,
    // <summary>Nombre d'événements en attente.</summary>
    int PendingEventsCount,
    // <summary>Nombre total de ressources.</summary>
    int ResourcesCount,
    // <summary>Nombre total de vues sur les ressources.</summary>
    int TotalResourceViews,
    // <summary>Nombre total de membres.</summary>
    int MembersCount,
    // <summary>Nombre d'abonnés newsletter actifs.</summary>
    int ActiveNewsletterCount,
    // <summary>Nombre total de commentaires.</summary>
    int CommentsCount,
    // <summary>Nombre total de projets.</summary>
    int ProjectsCount,
    // <summary>Nombre total de partenaires.</summary>
    int PartnersCount,
    // <summary>Nombre de certificats en attente.</summary>
    int PendingCertificatesCount);
