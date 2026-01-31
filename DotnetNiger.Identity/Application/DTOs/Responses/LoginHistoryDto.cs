namespace DotnetNiger.Identity.Application.DTOs.Responses;

public class LoginHistoryDto
{
    public Guid Id { get; set; }
    public DateTime LoginAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
