using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services.Interfaces;
using System.Security.Cryptography;
using System.Net.Mail;

namespace DotnetNiger.Community.Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterSubscriptionPersistence _newsletterRepository;
    private readonly IIdentityApiClient _identityApiClient;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        INewsletterSubscriptionPersistence newsletterRepository,
        IIdentityApiClient identityApiClient,
        ILogger<NewsletterService> logger)
    {
        _newsletterRepository = newsletterRepository;
        _identityApiClient = identityApiClient;
        _logger = logger;
    }

    public async Task<NewsletterSubscriptionResponse?> SubscribeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existing = await _newsletterRepository.GetByEmailAsync(normalizedEmail);
        if (existing != null)
        {
            // Reactivate safely without exposing tokens in public API responses.
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.IsVerified = false;
                existing.VerifiedAt = null;
                existing.UnsubscribedAt = null;
                existing.VerificationToken = GenerateVerificationToken();
                await _newsletterRepository.UpdateAsync(existing);
            }

            return MapToResponse(existing);
        }

        var subscription = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationToken = GenerateVerificationToken()
        };

        await _newsletterRepository.AddAsync(subscription);

        _logger.LogInformation("Newsletter subscription created for email: {Email}", normalizedEmail);

        return MapToResponse(subscription);
    }

    public async Task<bool> VerifySubscriptionAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var subscription = await _newsletterRepository.GetByVerificationTokenAsync(token);
        if (subscription == null)
            return false;

        subscription.IsVerified = true;
        subscription.VerifiedAt = DateTime.UtcNow;
        // Rotate token and keep it as management token for secure unsubscribe.
        subscription.VerificationToken = GenerateVerificationToken();

        await _newsletterRepository.UpdateAsync(subscription);

        _logger.LogInformation("Newsletter subscription verified for email: {Email}", subscription.Email);

        return true;
    }

    public async Task<bool> UnsubscribeAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        email = email.ToLowerInvariant().Trim();

        var subscription = await _newsletterRepository.GetByEmailAsync(email);
        if (subscription == null)
            return false;

        if (!string.Equals(subscription.VerificationToken, token, StringComparison.Ordinal))
            return false;

        subscription.IsActive = false;
        subscription.UnsubscribedAt = DateTime.UtcNow;
        subscription.VerificationToken = GenerateVerificationToken();

        await _newsletterRepository.UpdateAsync(subscription);

        _logger.LogInformation("Unsubscribed from newsletter: {Email}", email);

        return true;
    }

    public async Task<RegisterAndSubscribeResponse?> RegisterAndSubscribeAsync(RegisterAndSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var email = request.Email.ToLower().Trim();

        try
        {
            var username = email.Split('@')[0];
            var registeredUserId = await _identityApiClient.RegisterAsync(new IdentityRegisterRequest
            {
                Username = username,
                Email = email,
                Password = request.Password,
                FullName = request.FullName,
                Country = string.Empty,
                City = string.Empty
            });

            if (registeredUserId == null || registeredUserId == Guid.Empty)
            {
                _logger.LogWarning("Failed to register user in Identity service for email: {Email}", email);
                return new RegisterAndSubscribeResponse
                {
                    Message = "Failed to create account",
                    Email = email,
                    NewsletterSubscribed = false,
                    UserId = Guid.Empty
                };
            }

            var userId = registeredUserId.Value;

            // Subscribe to newsletter if requested
            bool newsletterSubscribed = false;
            if (request.SubscribeToNewsletter)
            {
                var existingSubscription = await _newsletterRepository.GetByEmailAsync(email);
                if (existingSubscription != null)
                {
                    existingSubscription.IsActive = true;
                    existingSubscription.IsVerified = true;
                    existingSubscription.VerifiedAt = DateTime.UtcNow;
                    existingSubscription.UnsubscribedAt = null;
                    await _newsletterRepository.UpdateAsync(existingSubscription);
                }
                else
                {
                    var subscription = new NewsletterSubscription
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        IsVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VerifiedAt = DateTime.UtcNow
                    };

                    await _newsletterRepository.AddAsync(subscription);
                }

                newsletterSubscribed = true;

                _logger.LogInformation("User registered and subscribed to newsletter: {Email}", email);
            }

            return new RegisterAndSubscribeResponse
            {
                UserId = userId,
                Email = email,
                NewsletterSubscribed = newsletterSubscribed,
                Message = "Registration successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering and subscribing user: {Email}", email);
            throw;
        }
    }

    public async Task<List<NewsletterSubscriptionResponse>> GetActiveSubscriptionsAsync()
    {
        var subscriptions = await _newsletterRepository.GetActiveSubscriptionsAsync();
        return subscriptions.Select(MapToResponse).ToList();
    }

    public async Task<List<NewsletterSubscriptionResponse>> GetSubscriptionsAsync(bool? isActive = null, bool? isVerified = null)
    {
        var subscriptions = await _newsletterRepository.GetSubscriptionsAsync(isActive, isVerified);
        return subscriptions.Select(MapToResponse).ToList();
    }

    public async Task<bool> SetSubscriptionStatusAsync(Guid subscriptionId, bool isActive, bool? isVerified = null)
    {
        var subscription = await _newsletterRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            return false;
        }

        subscription.IsActive = isActive;
        subscription.UnsubscribedAt = isActive ? null : DateTime.UtcNow;

        if (isVerified.HasValue)
        {
            subscription.IsVerified = isVerified.Value;
            subscription.VerifiedAt = isVerified.Value ? DateTime.UtcNow : null;
        }

        await _newsletterRepository.UpdateAsync(subscription);
        _logger.LogInformation("Admin updated newsletter subscription status. SubscriptionId: {SubscriptionId}, IsActive: {IsActive}, IsVerified: {IsVerified}",
            subscriptionId, isActive, isVerified);

        return true;
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid subscriptionId)
    {
        var subscription = await _newsletterRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            return false;
        }

        await _newsletterRepository.DeleteAsync(subscriptionId);
        _logger.LogInformation("Admin deleted newsletter subscription. SubscriptionId: {SubscriptionId}", subscriptionId);
        return true;
    }

    private string GenerateVerificationToken()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private NewsletterSubscriptionResponse MapToResponse(NewsletterSubscription subscription)
    {
        return new NewsletterSubscriptionResponse
        {
            Id = subscription.Id,
            Email = subscription.Email,
            IsVerified = subscription.IsVerified,
            IsActive = subscription.IsActive,
            CreatedAt = subscription.CreatedAt
        };
    }

}
