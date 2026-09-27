namespace DotnetNiger.Application.Common;

/// <summary>
/// Page de résultats accompagnée des métadonnées nécessaires au rendu d'un sélecteur de pages.
/// </summary>
/// <typeparam name="T">Type des éléments.</typeparam>
/// <param name="Items">Éléments de la page courante.</param>
/// <param name="PageNumber">Numéro de page, à partir de 1.</param>
/// <param name="PageSize">Nombre d'éléments par page.</param>
/// <param name="TotalCount">Nombre total d'éléments correspondant au filtre.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Obtient le nombre total de pages.
    /// </summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Obtient une valeur indiquant s'il existe une page précédente.
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Obtient une valeur indiquant s'il existe une page suivante.
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Projette les éléments de la page en un autre type, en conservant les métadonnées.
    /// </summary>
    /// <typeparam name="TResult">Type de sortie.</typeparam>
    /// <param name="selector">Projection appliquée à chaque élément.</param>
    /// <returns>Nouveau résultat projeté.</returns>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector)
        => new([.. Items.Select(selector)], PageNumber, PageSize, TotalCount);
}
