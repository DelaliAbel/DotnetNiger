namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de mise à jour du statut d'équipe d'un utilisateur.</summary>
public class UpdateUserTeamRequest
{
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
}
