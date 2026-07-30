namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de mise à jour du profil utilisateur.</summary>
public class UpdateProfileRequest
{
    /// <summary>Nom complet.</summary>
    public string? FullName { get; set; }
    /// <summary>Numéro de téléphone.</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Biographie.</summary>
    public string? Bio { get; set; }
    /// <summary>URL de l'avatar.</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Pays de résidence.</summary>
    public string? Country { get; set; }
    /// <summary>Ville de résidence.</summary>
    public string? City { get; set; }
    /// <summary>Indique si c'est un membre de l'équipe.</summary>
    public bool? IsTeamMember { get; set; }
    /// <summary>Poste occupé.</summary>
    public string? Position { get; set; }
    /// <summary>Compétences techniques.</summary>
    public List<string>? Skills { get; set; }
}
