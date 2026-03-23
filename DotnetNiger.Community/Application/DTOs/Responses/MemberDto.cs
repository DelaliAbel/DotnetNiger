namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Data Transfer Object for Member entity.
/// Used in GET endpoints to return member information.
/// </summary>
public class MemberDto
{
    /// <summary>Gets or sets the unique identifier for the member.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the member's full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the member's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the member's phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the member's role/position.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets the member's bio or description.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the member's avatar URL.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the member is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the date the member was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the date the member was last updated.</summary>
    public DateTime UpdatedAt { get; set; }
}
