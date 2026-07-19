namespace DotnetNiger.Domain.Models;

/// <summary>Ticket à usage unique pour le login externe frontend (stocké en cache).</summary>
public class ExternalLoginTicket
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
