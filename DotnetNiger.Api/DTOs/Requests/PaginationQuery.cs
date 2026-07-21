namespace DotnetNiger.Api.DTOs.Requests;

public record PaginationQuery(int Page = 1, int PageSize = 20)
{
    public int EnsurePage => Math.Max(1, Page);
    public int EnsurePageSize => Math.Clamp(PageSize, 1, 100);
}
