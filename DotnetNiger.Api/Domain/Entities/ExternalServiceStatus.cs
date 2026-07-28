namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Statuts possibles d'un service externe.
/// </summary>
public enum ExternalServiceStatus
{
    /// <summary>En attente de validation.</summary>
    Pending,
    /// <summary>Service actif et fonctionnel.</summary>
    Active,
    /// <summary>Service suspendu temporairement.</summary>
    Suspended,
    /// <summary>Service définitivement retiré.</summary>
    Removed
}
