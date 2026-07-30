using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un commentaire.</summary>
public class CreateCommentRequest
{
    /// <summary>Contenu du commentaire.</summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>Identifiant de l'article commenté.</summary>
    public Guid? PostId { get; set; }
    /// <summary>Identifiant de l'événement commenté.</summary>
    public Guid? EventId { get; set; }
    /// <summary>Identifiant du commentaire parent (réponse).</summary>
    public Guid? ParentCommentId { get; set; }
}
