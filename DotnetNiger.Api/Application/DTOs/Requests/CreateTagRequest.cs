using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un tag.</summary>
public class CreateTagRequest
{
    /// <summary>Nom du tag (max 100 caractères).</summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
