namespace DotnetNiger.Web.Controller.Contracts;

internal static partial class ApiProjections
{
    /// <summary>
    /// Construit une page de réponses à partir d'un résultat paginé du domaine.
    /// </summary>
    /// <typeparam name="TSource">Type des entités.</typeparam>
    /// <typeparam name="TResult">Type des réponses.</typeparam>
    /// <param name="result">Résultat paginé.</param>
    /// <param name="selector">Projection appliquée à chaque entité.</param>
    /// <returns>Page sérialisable.</returns>
    public static PageResponse<TResult> ToPage<TSource, TResult>(
        Application.Common.PagedResult<TSource> result,
        Func<TSource, TResult> selector) =>
        new(
            [.. result.Items.Select(selector)],
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasNext);

}
