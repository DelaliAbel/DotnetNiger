// Contrat domaine Identity: IAuditableEntity
namespace DotnetNiger.Identity.Domain.Interfaces;

/// <summary>
/// Contrat pour les entites avec suivi de creation et modification.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Date de creation de l'entite.
    /// </summary>
    DateTime CreatedAt { get; set; }
}
