using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'ajout d'un lien social au profil.</summary>
public class AddSocialLinkRequest
{
    /// <summary>Nom de la plateforme (GitHub, LinkedIn, etc.).</summary>
    [Required]
    public string Platform { get; set; } = string.Empty;

    /// <summary>URL du profil social.</summary>
    [Required, Url]
    public string Url { get; set; } = string.Empty;
}
