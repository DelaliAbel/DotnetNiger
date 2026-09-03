namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un événement organisé par le DotnetNiger.
/// </summary>
public class Event
{
    /// <summary>Identifiant unique de l'événement.</summary>
    public Guid Id { get; set; }
    /// <summary>Titre de l'événement.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description de l'événement.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Lieu de l'événement.</summary>
    public string Location { get; set; } = string.Empty;
    /// <summary>Type d'événement (meetup, conférence, hackathon).</summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>Catégorie de l'événement.</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>Statut de l'événement.</summary>
    public EventStatus Status { get; set; } = EventStatus.Draft;
    /// <summary>Date et heure de début.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Date et heure de fin.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>URL de l'image de couverture.</summary>
    public string CoverImageUrl { get; set; } = string.Empty;
    /// <summary>Identifiant du créateur.</summary>
    public Guid CreatedBy { get; set; }
    /// <summary>Identifiant de l'organisateur.</summary>
    public Guid OrganizerId { get; set; }
    /// <summary>Nom de l'organisateur.</summary>
    public string OrganizerName { get; set; } = string.Empty;
    /// <summary>Capacité maximale de participants.</summary>
    public int Capacity { get; set; }
    /// <summary>Nombre d'inscrits actuel.</summary>
    public int RegisteredCount { get; set; }
    /// <summary>Lien de réunion en ligne.</summary>
    public string MeetupLink { get; set; } = string.Empty;
    /// <summary>Raison du rejet éventuel.</summary>
    public string? RejectionReason { get; set; }
    /// <summary>Date de soumission pour revue.</summary>
    public DateTime? SubmittedAt { get; set; }
    /// <summary>Date de publication.</summary>
    public DateTime? PublishedAt { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation vers l'organisateur.</summary>
    public ApplicationUser? Organizer { get; set; }
    /// <summary>Médias de l'événement.</summary>
    public ICollection<EventMedia> Medias { get; set; } = [];
    /// <summary>Inscriptions à l'événement.</summary>
    public ICollection<EventRegistration> Registrations { get; set; } = [];
    /// <summary>Commentaires de l'événement.</summary>
    public ICollection<Comment> Comments { get; set; } = [];
    /// <summary>Tags de l'événement.</summary>
    public ICollection<EventTag> EventTags { get; set; } = [];
    /// <summary>Intervenants de l'événement.</summary>
    public ICollection<Speaker> Speakers { get; set; } = [];
}

public enum EventStatus
{
    Draft,
    PendingReview,
    Published,
    Unpublished,
    Rejected,
    Cancelled,
    Archived
}
