using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Pages.Account;

[EnableRateLimiting("Auth")]
public class RegisterModel : PageModel
{
    private static readonly char[] CodeChars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789".ToCharArray();

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IdentityDbContext _db;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender<ApplicationUser> _emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IdentityDbContext db,
        ILogger<RegisterModel> logger,
        IEmailSender<ApplicationUser> emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _logger = logger;
        _emailSender = emailSender;
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
        {
            return await HandleCodeConfirmationAsync();
        }

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

        var existingUser = await _userManager.FindByEmailAsync(Email);
        if (existingUser != null)
        {
            ErrorMessage = "Un compte avec cet email existe déjà.";
            return Page();
        }

        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
        {
            ErrorMessage = "Aucun tenant configuré. Veuillez contacter l'administrateur.";
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            FirstName = FirstName,
            LastName = LastName,
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            return Page();
        }

        await _userManager.AddToRoleAsync(user, "User");
        _logger.LogInformation("New user registered (pending confirmation): {Email}", Email);

        var code = GenerateCode();
        user.EmailConfirmationCode = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        try
        {
            await SendConfirmationEmailAsync(user, code, tenant.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send confirmation email to {Email}. User can request a new code.", Email);
        }

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
            ErrorMessage = "Utilisateur non trouvé. Veuillez vous réinscrire.";
            return Page();
        }

        if (user.EmailConfirmed)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return await RedirectOrSuccessAsync();
        }

        if (user.EmailConfirmationCode == null || user.EmailConfirmationCodeExpiry == null)
        {
            ErrorMessage = "Aucun code de confirmation trouvé. Veuillez demander un nouveau code.";
            ShowCodeForm = true;
            PendingEmail = email;
            return Page();
        }

        if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
        {
            ErrorMessage = "Code de confirmation expiré. Veuillez demander un nouveau code.";
            ShowCodeForm = true;
            PendingEmail = email;
            return Page();
        }

        var hashedCode = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ConfirmationCode)));
        if (!string.Equals(user.EmailConfirmationCode, hashedCode, StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Code de confirmation invalide. Veuillez vérifier le code reçu par email.";
            ShowCodeForm = true;
            PendingEmail = email;
            return Page();
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("Email confirmed for {Email}", email);

        return await RedirectOrSuccessAsync();
    }

    public async Task<IActionResult> OnPostResendCodeAsync()
    {
        await LoadExternalProvidersAsync();

        var email = PendingEmail ?? Email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ErrorMessage = "Utilisateur non trouvé.";
            return Page();
        }

        if (user.EmailConfirmed)
        {
            SuccessMessage = "Email déjà confirmé. Vous pouvez vous connecter.";
            return Page();
        }

        var tenant = await _db.Tenants.FindAsync(user.TenantId);
        var code = GenerateCode();
        user.EmailConfirmationCode = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        try
        {
            await SendConfirmationEmailAsync(user, code, tenant?.Name ?? "Plateforme");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resend confirmation email to {Email}.", email);
        }

        ShowCodeForm = true;
        PendingEmail = email;
        SuccessMessage = $"Un nouveau code a été envoyé à {email}.";
        return Page();
    }

    private async Task<IActionResult> RedirectOrSuccessAsync()
    {
        if (ReturnUrl == "/" || string.IsNullOrEmpty(ReturnUrl))
        {
            SuccessMessage = "Compte créé et confirmé avec succès !";
            return Page();
        }

        var separator = ReturnUrl.Contains('?') ? '&' : '?';
        var fullName = $"{FirstName} {LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var user = await _userManager.FindByEmailAsync(PendingEmail ?? Email);
            fullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "";
        }
        var redirectUrl = $"{ReturnUrl}{separator}userId={User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}&email={Uri.EscapeDataString(PendingEmail ?? Email)}&fullName={Uri.EscapeDataString(fullName)}";
        return Url.IsLocalUrl(redirectUrl) ? LocalRedirect(redirectUrl) : Redirect(redirectUrl);
    }

    private async Task LoadExternalProvidersAsync()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        ExternalProviders = schemes
            .Where(s => !string.IsNullOrEmpty(s.DisplayName))
            .Select(s => new ExternalProvider { Name = s.Name, DisplayName = s.DisplayName! })
            .ToList();
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string code, string tenantName)
    {
        if (_emailSender is EmailSender typed)
        {
            var confirmUrl = $"{Request.Scheme}://{Request.Host}/Account/Register?email={Uri.EscapeDataString(user.Email!)}&code={Uri.EscapeDataString(code)}";
            await typed.SendConfirmationCodeAsync(user, user.Email!, code, confirmUrl, tenantName);
        }
    }

    private static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = CodeChars[bytes[i] % CodeChars.Length];
        return new string(code);
    }
}

public class ExternalProvider
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
