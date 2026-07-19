using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Domain.Models;

namespace DotnetNiger.Server.Pages.Account;

public partial class LoginModel
{
    public IActionResult OnPostExternalLogin(string provider, string? returnUrl)
    {
        returnUrl ??= "/";
        var redirectUrl = Url.Page("./Login", pageHandler: "ExternalCallback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetExternalCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= "/";
        _logger.LogInformation("ExternalCallback: returnUrl={ReturnUrl}, remoteError={RemoteError}", returnUrl, remoteError);

        if (remoteError != null)
        {
            _logger.LogWarning("External callback remote error: {RemoteError}", remoteError);
            ErrorMessage = $"Erreur du fournisseur externe : {remoteError}";
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        try
        {
            var (user, roles) = await _authService.HandleExternalLoginAsync("external");
            return SafeOrTicketRedirect(user, returnUrl, roles.ToList());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("External login failed: {Message}", ex.Message);
            ErrorMessage = ex.Message;
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }
    }

    private string FrontendUrl() =>
        HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
            .GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5201";

    private IActionResult SafeLocalRedirect(string url)
    {
        if (url == "/" || string.IsNullOrEmpty(url))
            return Redirect(FrontendUrl());
        return Url.IsLocalUrl(url) ? LocalRedirect(url) : Redirect(FrontendUrl());
    }

    private IActionResult SafeOrTicketRedirect(ApplicationUser user, string returnUrl, List<string> roles)
    {
        if (returnUrl == "/" || string.IsNullOrEmpty(returnUrl))
            return RedirectToFrontendWithTicket(user, FrontendUrl());
        if (Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToFrontendWithTicket(user, returnUrl);
    }

    private IActionResult RedirectToFrontendWithTicket(ApplicationUser user, string returnUrl)
    {
        var ticket = Guid.NewGuid().ToString("N");
        var cacheEntry = new ExternalLoginTicket
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive
        };
        _cache.Set($"external_login_{ticket}", cacheEntry, TimeSpan.FromMinutes(5));

        var separator = returnUrl.Contains('?') ? '&' : '?';
        return Redirect($"{returnUrl}{separator}ticket={ticket}");
    }
}
