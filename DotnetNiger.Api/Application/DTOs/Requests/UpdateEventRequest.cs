using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un événement.</summary>
public class UpdateEventRequest
{
    /// <summary>Titre de l'événement (max 200 caractères).</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Slug URL de l'événement.</summary>
    public string? Slug { get; set; }

    /// <summary>Description de l'événement.</summary>
    public string? Description { get; set; }

    /// <summary>Date et heure de début.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Date et heure de fin.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Lieu de l'événement.</summary>
    public string? Location { get; set; }

    /// <summary>URL de l'image de couverture.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Type d'événement.</summary>
    public string? EventType { get; set; }

    /// <summary>Catégorie de l'événement.</summary>
    public string? Category { get; set; }

    /// <summary>Nom de l'organisateur.</summary>
    public string? OrganizerName { get; set; }

    /// <summary>Capacité maximale.</summary>
    public int? Capacity { get; set; }

    /// <summary>Lien de visioconférence.</summary>
    public string? MeetupLink { get; set; }

    /// <summary>Indique si l'événement est publié.</summary>
    public bool? IsPublished { get; set; }

    /// <summary>Indique si l'événement est archivé.</summary>
    public bool? IsArchived { get; set; }

    /// <summary>Noms des tags associés.</summary>
    public List<string>? TagNames { get; set; }

    /// <summary>URLs des images de la galerie.</summary>
    public List<string>? GalleryImageUrls { get; set; }

    /// <summary>Liste des intervenants.</summary>
    public List<SpeakerRequest>? Speakers { get; set; }
}
