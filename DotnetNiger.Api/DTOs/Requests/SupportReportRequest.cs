namespace DotnetNiger.Api.DTOs.Requests;

public class SupportReportRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Type { get; set; }
    public string? Steps { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAgent { get; set; }
}
