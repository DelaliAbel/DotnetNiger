namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de pagination.</summary>
public record PaginationQuery(
    // <summary>Numéro de page (commence à 1).</summary>
    int Page = 1,
    // <summary>Nombre d'éléments par page.</summary>
    int PageSize = 20)
{
    /// <summary>Numéro de page validé (minimum 1).</summary>
    public int EnsurePage => Math.Max(1, Page);
    /// <summary>Taille de page validée (entre 1 et 100).</summary>
    public int EnsurePageSize => Math.Clamp(PageSize, 1, 100);
}
