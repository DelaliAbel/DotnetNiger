using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Application.Common;

/// <summary>
/// Demande de pagination normalisée, indépendante du provider.
/// </summary>
/// <remarks>
/// Les bornes sont appliquées ici plutôt que dans les contrôleurs : une taille de page
/// illimitée permettrait d'extraction la base entière en mémoire.
/// </remarks>
public readonly record struct PageRequest
{
    /// <summary>
    /// Taille de page par défaut.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Taille de page maximale.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Initialise une demande de pagination.
    /// </summary>
    /// <param name="pageNumber">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <exception cref="DomainException">
    /// Le numéro de page est inférieur à 1, ou la taille de page est hors bornes.
    /// </exception>
    public PageRequest(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new DomainException("page_number_invalid", "Le numéro de page doit être supérieur ou égal à 1.");
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            throw new DomainException(
                "page_size_invalid",
                $"La taille de page doit être comprise entre 1 et {MaxPageSize}.");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Obtient le numéro de page, à partir de 1.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Obtient le nombre d'éléments par page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Obtient le nombre d'éléments à ignorer pour cette page.
    /// </summary>
    public int Skip => checked((PageNumber - 1) * PageSize);
}
