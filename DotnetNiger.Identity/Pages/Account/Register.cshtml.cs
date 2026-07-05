using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using DotnetNiger.Common.Constants;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Application.Services;

namespace DotnetNiger.Identity.Pages.Account;

[EnableRateLimiting("Auth")]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AccountService _accountService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AccountService accountService,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _accountService = accountService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmationCode { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool ShowCodeForm { get; set; }
    public string? PendingEmail { get; set; }

    public List<ExternalProvider> ExternalProviders { get; set; } = [];

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Redirect(string.IsNullOrEmpty(ReturnUrl) ? "/" : ReturnUrl);
            return;
        }

        ReturnUrl ??= "/";
        await LoadExternalProvidersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= "/";
        await LoadExternalProvidersAsync();

        if (!string.IsNullOrWhiteSpace(ConfirmationCode))
            return await HandleCodeConfirmationAsync();

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Le prénom et le nom sont requis.";
            return Page();
        }

        try
        {
            await _accountService.RegisterAsync(Email, Password, FirstName, LastName);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }

        _logger.LogInformation("New user registered (pending confirmation): {Email}", Email);
        ShowCodeForm = true;
        PendingEmail = Email;
        SuccessMessage = $"Un email de confirmation a été envoyé à {Email}. Veuillez entrer le code reçu pour activer votre compte.";
        return Page();
    }

    private async Task<IActionResult> HandleCodeConfirmationAsync()
    {
        var email = PendingEmail ?? Email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ErrorMessage = ErrorMessages.UserNotFound;
            return Page();
        }

        if (user.EmailConfirmed)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return await RedirectOrSuccessAsync(email);
        }

        try
        {
            await _accountService.ConfirmEmailAsync(email, ConfirmationCode);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            ShowCodeForm = true;
            PendingEmail = email;
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("Email confirmed for {Email}", email);

        return await RedirectOrSuccessAsync(email);
    }

    public async Task<IActionResult> OnPostResendCodeAsync()
    {
        await LoadExternalProvidersAsync();
        var email = PendingEmail ?? Email;

        try
        {
            await _accountService.ResendConfirmationCodeAsync(email);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }

        ShowCodeForm = true;
        PendingEmail = email;
        SuccessMessage = $"Un nouveau code a été envoyé à {email}.";
        return Page();
    }

    private async Task<IActionResult> RedirectOrSuccessAsync(string email)
    {
        if (ReturnUrl == "/" || string.IsNullOrEmpty(ReturnUrl))
        {
            SuccessMessage = "Compte créé et confirmé avec succès !";
            return Page();
        }

        var fullName = $"{FirstName} {LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var user = await _userManager.FindByEmailAsync(email);
            fullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "";
        }
        var separator = ReturnUrl.Contains('?') ? '&' : '?';
        var redirectUrl = $"{ReturnUrl}{separator}userId={User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}&email={Uri.EscapeDataString(email)}&fullName={Uri.EscapeDataString(fullName)}";
        return Url.IsLocalUrl(redirectUrl) ? LocalRedirect(redirectUrl) : Redirect(redirectUrl);
    }

    private async Task LoadExternalProvidersAsync()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        ExternalProviders = schemes
            .Where(s => !string.IsNullOrEmpty(s.DisplayName) && s.Name != "SmartScheme")
            .Select(s => new ExternalProvider { Name = s.Name, DisplayName = s.DisplayName! })
            .ToList();
    }
}

public class ExternalProvider
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
