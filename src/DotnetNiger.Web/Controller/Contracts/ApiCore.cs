namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>
/// Projets de l'API publique.
/// </summary>
public static class ApiRoutes
{
    /// <summary>
    /// Préfixe commun des routes. Aucun versionnement n'est appliqué : le contrat est en
    /// construction et n'a pas encore été publié à des clients qu'il serait coûteux de
    /// rompre.
    /// </summary>
    public const string Base = "/api";
}

/// <summary>
/// Paramètres de pagination acceptés par les listes.
/// </summary>
/// <param name="Page">Numéro de page, à partir de 1.</param>
/// <param name="PageSize">Nombre d'éléments par page.</param>
public readonly record struct PageQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Page de résultats exposée par l'API.
/// </summary>
/// <typeparam name="T">Type des éléments.</typeparam>
/// <param name="Items">Éléments de la page.</param>
/// <param name="Page">Numéro de page.</param>
/// <param name="PageSize">Taille de page.</param>
/// <param name="TotalCount">Nombre total d'éléments correspondant au filtre.</param>
/// <param name="TotalPages">Nombre total de pages.</param>
/// <param name="HasNext">Vrai s'il existe une page suivante.</param>
public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNext);

