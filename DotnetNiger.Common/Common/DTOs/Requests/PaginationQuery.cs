namespace DotnetNiger.Common.DTOs.Requests;

/// <summary>Paramètres de pagination pour les requêtes listant des éléments.</summary>
public record PaginationQuery(int Page = 1, int PageSize = 20)
{
    public int EnsurePage => Math.Max(1, Page);
    public int EnsurePageSize => Math.Clamp(PageSize, 1, 100);
}
