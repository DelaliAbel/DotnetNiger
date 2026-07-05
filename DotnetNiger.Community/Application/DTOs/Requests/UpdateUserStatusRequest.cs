namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de mise à jour du statut d'un utilisateur.</summary>
public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}
