namespace DotnetNiger.Domain.DTOs.Responses;

public record NewsletterSubscriptionResponse(
    Guid Id, string Email, string Name, bool IsActive, DateTime SubscribedAt);
