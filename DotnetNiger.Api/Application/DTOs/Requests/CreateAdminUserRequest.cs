using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un compte administrateur.</summary>
public class CreateAdminUserRequest
{
    /// <summary>Nom complet de l'administrateur.</summary>
    [Required]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Adresse e-mail de l'administrateur.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mot de passe de l'administrateur.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Indique si c'est un membre de l'équipe.</summary>
    public bool IsTeamMember { get; set; }
    /// <summary>Poste de l'administrateur.</summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>Indique si c'est un collaborateur.</summary>
    public bool IsCollaborator { get; set; }
    /// <summary>Indique si c'est un administrateur.</summary>
    public bool IsAdmin { get; set; }
    /// <summary>Indique si le certificat est approuvé.</summary>
    public bool HasApprovedCertificate { get; set; }
}
