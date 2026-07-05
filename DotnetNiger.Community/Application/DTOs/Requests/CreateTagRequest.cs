using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de création d'un tag.</summary>
public class CreateTagRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
