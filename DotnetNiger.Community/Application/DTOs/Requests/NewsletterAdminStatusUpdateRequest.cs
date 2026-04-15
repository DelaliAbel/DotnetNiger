namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request to update newsletter subscription status by admins.
/// </summary>
public class NewsletterAdminStatusUpdateRequest
{
    public bool IsActive { get; set; }
    public bool? IsVerified { get; set; }
}
