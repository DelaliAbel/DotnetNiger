using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'un événement.</summary>
public class CreateEventRequest
{
    /// <summary>Titre de l'événement.</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>Slug URL de l'événement.</summary>
    public string? Slug { get; set; }

    /// <summary>Description détaillée de l'événement.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>Lieu de l'événement.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Type d'événement (meetup, conférence, etc.).</summary>
    [Required]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Catégorie de l'événement.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Date et heure de début.</summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>Date et heure de fin.</summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>URL de l'image de couverture.</summary>
    public string CoverImageUrl { get; set; } = string.Empty;
    /// <summary>Nom de l'organisateur.</summary>
    public string OrganizerName { get; set; } = string.Empty;
    /// <summary>Capacité maximale de participants.</summary>
    public int Capacity { get; set; } = 100;
    /// <summary>Lien de visioconférence.</summary>
    public string MeetupLink { get; set; } = string.Empty;
    /// <summary>Indique si l'événement est publié.</summary>
    public bool IsPublished { get; set; }
    /// <summary>Indique si l'événement est archivé.</summary>
    public bool IsArchived { get; set; }
    /// <summary>Noms des tags associés.</summary>
    public List<string> TagNames { get; set; } = [];
    /// <summary>Identifiants des tags associés.</summary>
    public List<Guid> TagIds { get; set; } = [];
    /// <summary>URLs des images de la galerie.</summary>
    public List<string> GalleryImageUrls { get; set; } = [];
    /// <summary>Liste des intervenants de l'événement.</summary>
    public List<SpeakerRequest> Speakers { get; set; } = [];
}

/// <summary>Requête d'ajout d'un intervenant à un événement.</summary>
public class SpeakerRequest
{
    /// <summary>Identifiant de l'utilisateur intervenant.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de l'intervenant.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Rôle de l'intervenant.</summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>URL de l'avatar de l'intervenant.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
}
