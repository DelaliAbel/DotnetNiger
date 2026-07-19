using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un commentaire.</summary>
public class UpdateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
