namespace DotnetNiger.Community.Domain.Entities;

public class EventRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; } // FK Identity API
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsAttended { get; set; } = false;
    public string RegistrationStatus { get; set; } = string.Empty; // Registered, Attended, Cancelled

    // FK
    public Event Event { get; set; } = null!;
}
