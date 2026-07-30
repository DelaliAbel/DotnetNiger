using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un commentaire.</summary>
public class UpdateCommentRequest
{
    /// <summary>Nouveau contenu du commentaire.</summary>
    [Required]
    public string Content { get; set; } = string.Empty;
}
