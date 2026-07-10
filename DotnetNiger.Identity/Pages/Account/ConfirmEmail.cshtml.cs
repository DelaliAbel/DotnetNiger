using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Application.Services;

namespace DotnetNiger.Identity.Pages.Account;

[EnableRateLimiting("Auth")]
public class ConfirmEmailModel : PageModel
{
    private readonly AccountService _accountService;

    public ConfirmEmailModel(AccountService accountService)
    {
        _accountService = accountService;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmationCode { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool ShowCodeForm { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrWhiteSpace(ConfirmationCode))
            return await HandleCodeConfirmationAsync();

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Veuillez saisir votre adresse email.";
            return Page();
        }

        try
        {
            await _accountService.ResendConfirmationCodeAsync(Email);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }

        ShowCodeForm = true;
        SuccessMessage = $"Un code de confirmation a ete envoye a {Email}.";
        return Page();
    }

    private async Task<IActionResult> HandleCodeConfirmationAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Veuillez saisir votre adresse email.";
            return Page();
        }

        try
        {
            await _accountService.ConfirmEmailAsync(Email, ConfirmationCode);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            ShowCodeForm = true;
            return Page();
        }

        SuccessMessage = "Email confirme avec succes ! Vous pouvez maintenant vous connecter.";
        ShowCodeForm = false;
        return Page();
    }

    public async Task<IActionResult> OnPostResendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Veuillez saisir votre adresse email.";
            return Page();
        }

        try
        {
            await _accountService.ResendConfirmationCodeAsync(Email);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }

        ShowCodeForm = true;
        SuccessMessage = $"Un nouveau code a ete envoye a {Email}.";
        return Page();
    }
}
