namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un fichier média associé à un événement.
/// </summary>
public class EventMedia
{
    /// <summary>Identifiant unique du média.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'événement.</summary>
    public Guid EventId { get; set; }
    /// <summary>Type de média (image, vidéo, audio).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>URL du fichier.</summary>
    public string FileUrl { get; set; } = string.Empty;
    /// <summary>Type MIME du fichier.</summary>
    public string FileType { get; set; } = string.Empty;
    /// <summary>URL alternative du média.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>Titre du média.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Navigation vers l'événement.</summary>
    public Event Event { get; set; } = null!;
}
