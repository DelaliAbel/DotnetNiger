using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.DTOs.Requests;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Newsletter management endpoints for public subscription and registration
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/newsletters")]
public class NewsletterController : ApiControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        INewsletterService newsletterService,
        ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    /// <summary>
    /// Subscribe an email to the newsletter
    /// </summary>
    /// <param name="request">Email to subscribe</param>
    /// <returns>Generic success response to avoid leaking account/subscription state</returns>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeToNewsletterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        try
        {
            _ = await _newsletterService.SubscribeAsync(request.Email);

            _logger.LogInformation("Newsletter subscription request: {Email}", request.Email);

            return SuccessMessage("If the email is valid, the subscription request has been processed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to newsletter: {Email}", request.Email);
            return Problem(detail: "Failed to subscribe", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Quick subscribe for visitors with email only.
    /// </summary>
    [HttpPost("quick-subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> QuickSubscribe([FromBody] SubscribeToNewsletterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        try
        {
            _ = await _newsletterService.SubscribeAsync(request.Email);
            return SuccessMessage("If the email is valid, the subscription request has been processed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick subscribe newsletter flow");
            return Problem(detail: "Failed to process subscription", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Verify newsletter subscription using verification token
    /// </summary>
    /// <param name="request">Verification token</param>
    /// <returns>Success or failure</returns>
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifySubscription([FromBody] VerifyNewsletterSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Verification token is required");

        try
        {
            _ = await _newsletterService.VerifySubscriptionAsync(request.Token);

            _logger.LogInformation("Newsletter subscription verified with token");

            return SuccessMessage("If the token is valid, the email has been verified.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying newsletter subscription");
            return Problem(detail: "Failed to verify subscription", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Unsubscribe an email from the newsletter
    /// </summary>
    /// <param name="request">Email to unsubscribe</param>
    /// <returns>Success or failure</returns>
    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeFromNewsletterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Email and token are required");

        try
        {
            _ = await _newsletterService.UnsubscribeAsync(request.Email, request.Token);

            _logger.LogInformation("Unsubscribed from newsletter: {Email}", request.Email);

            return SuccessMessage("If the request is valid, unsubscription has been processed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from newsletter: {Email}", request.Email);
            return Problem(detail: "Failed to unsubscribe", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Register a new user account and optionally subscribe to newsletter
    /// Calls Identity API to create the account
    /// </summary>
    /// <param name="request">User registration and newsletter subscription details</param>
    /// <returns>Registration response with user ID</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAndSubscribe([FromBody] RegisterAndSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Email, password, and full name are required");

        try
        {
            var result = await _newsletterService.RegisterAndSubscribeAsync(request);
            if (result == null || result.UserId == Guid.Empty)
                return Conflict("Failed to create account or email already exists");

            _logger.LogInformation("User registered and designed: {Email}", request.Email);

            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Email}", request.Email);
            return Problem(detail: "Failed to register", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get all active newsletter subscriptions (Admin only)
    /// </summary>
    /// <returns>List of active subscriptions</returns>
    [HttpGet("subscriptions")]
    [Authorize(Policy = "AdminOrSuperAdmin")]
    public async Task<IActionResult> GetActiveSubscriptions()
    {
        try
        {
            var subscriptions = await _newsletterService.GetActiveSubscriptionsAsync();
            return Success(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching newsletter subscriptions");
            return Problem(detail: "Failed to fetch subscriptions", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get newsletter subscriptions with optional admin filters.
    /// </summary>
    [HttpGet("subscriptions/all")]
    [Authorize(Policy = "AdminOrSuperAdmin")]
    public async Task<IActionResult> GetSubscriptions([FromQuery] bool? isActive = null, [FromQuery] bool? isVerified = null)
    {
        try
        {
            var subscriptions = await _newsletterService.GetSubscriptionsAsync(isActive, isVerified);
            return Success(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filtered newsletter subscriptions");
            return Problem(detail: "Failed to fetch subscriptions", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Admin/SuperAdmin updates newsletter subscription state.
    /// </summary>
    [HttpPatch("subscriptions/{subscriptionId:guid}")]
    [Authorize(Policy = "AdminOrSuperAdmin")]
    public async Task<IActionResult> UpdateSubscriptionStatus(Guid subscriptionId, [FromBody] NewsletterAdminStatusUpdateRequest request)
    {
        try
        {
            var updated = await _newsletterService.SetSubscriptionStatusAsync(subscriptionId, request.IsActive, request.IsVerified);
            if (!updated)
            {
                return NotFoundProblem("Subscription not found.");
            }

            return SuccessMessage("Subscription updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating newsletter subscription. SubscriptionId: {SubscriptionId}", subscriptionId);
            return Problem(detail: "Failed to update subscription", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Admin/SuperAdmin deletes a newsletter subscription.
    /// </summary>
    [HttpDelete("subscriptions/{subscriptionId:guid}")]
    [Authorize(Policy = "AdminOrSuperAdmin")]
    public async Task<IActionResult> DeleteSubscription(Guid subscriptionId)
    {
        try
        {
            var deleted = await _newsletterService.DeleteSubscriptionAsync(subscriptionId);
            if (!deleted)
            {
                return NotFoundProblem("Subscription not found.");
            }

            return SuccessMessage("Subscription deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting newsletter subscription. SubscriptionId: {SubscriptionId}", subscriptionId);
            return Problem(detail: "Failed to delete subscription", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
