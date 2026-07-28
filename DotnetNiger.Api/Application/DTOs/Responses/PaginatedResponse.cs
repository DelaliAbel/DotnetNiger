using System.Text.Json.Serialization;

namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse paginée générique.</summary>
public class PaginatedResponse<T>
{
    /// <summary>Constructeur par défaut.</summary>
    public PaginatedResponse() { }

    /// <summary>Constructeur avec paramètres.</summary>
    public PaginatedResponse(IList<T> items, int totalCount, int page, int pageSize)
    {
        Items = [.. items];
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>Liste des éléments de la page courante.</summary>
    [JsonPropertyOrder(1)]
    public List<T> Items { get; set; } = [];

    /// <summary>Nombre total d'éléments.</summary>
    [JsonPropertyOrder(2)]
    public int TotalCount { get; set; }

    /// <summary>Numéro de la page courante.</summary>
    [JsonPropertyOrder(3)]
    public int Page { get; set; }

    /// <summary>Nombre d'éléments par page.</summary>
    [JsonPropertyOrder(4)]
    public int PageSize { get; set; }

    /// <summary>Nombre total de pages.</summary>
    [JsonPropertyOrder(5)]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));

    /// <summary>Indique s'il y a une page suivante.</summary>
    [JsonPropertyOrder(6)]
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Indique s'il y a une page précédente.</summary>
    [JsonPropertyOrder(7)]
    public bool HasPreviousPage => Page > 1;
}
