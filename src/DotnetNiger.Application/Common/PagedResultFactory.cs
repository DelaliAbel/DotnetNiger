namespace DotnetNiger.Application.Common;

/// <summary>
/// Fabriques de <see cref="PagedResult{T}"/>.
/// </summary>
/// <remarks>
/// Séparée de <see cref="PagedResult{T}"/> pour respecter CA1000, qui interdit les membres
/// statiques sur un type générique.
/// </remarks>
public static class PagedResultFactory
{
    /// <summary>
    /// Crée un résultat vide pour une page demandée.
    /// </summary>
    /// <typeparam name="T">Type des éléments.</typeparam>
    /// <param name="request">Demande de pagination d'origine.</param>
    /// <returns>Page vide.</returns>
    public static PagedResult<T> Empty<T>(PageRequest request)
        => new([], request.PageNumber, request.PageSize, 0);
}
