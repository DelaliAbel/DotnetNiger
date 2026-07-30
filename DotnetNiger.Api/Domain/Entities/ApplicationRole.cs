using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Rôle personnalisé de l'application, étend IdentityRole.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>Description du rôle.</summary>
    public string? Description { get; set; }
    /// <summary>Date de création du rôle.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
