namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de recherche multicritères.</summary>
public class SearchQueryRequest
{
    /// <summary>Terme de recherche.</summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>Type de contenu recherché (post, event, etc.).</summary>
    public string? Type { get; set; }
    /// <summary>Numéro de page.</summary>
    public int Page { get; set; } = 1;
    /// <summary>Nombre de résultats par page.</summary>
    public int PageSize { get; set; } = 10;
}
