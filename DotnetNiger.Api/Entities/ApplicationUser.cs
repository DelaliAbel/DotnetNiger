using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Entities;

/// <summary>
/// Utilisateur principal de l'application, étend IdentityUser.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Prénom de l'utilisateur.</summary>
    public string? FirstName { get; set; }
    /// <summary>Nom de famille de l'utilisateur.</summary>
    public string? LastName { get; set; }
    /// <summary>URL de l'avatar.</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Indique si le compte est actif.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Date de création du compte.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Code de confirmation d'email.</summary>
    public string? EmailConfirmationCode { get; set; }
    /// <summary>Date d'expiration du code de confirmation.</summary>
    public DateTime? EmailConfirmationCodeExpiry { get; set; }
    /// <summary>Email en attente de confirmation.</summary>
    public string? PendingEmail { get; set; }
}
