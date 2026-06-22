using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class InviteModel : BasePageModel
{
    public InviteModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty]
    public InviteInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/admin/invite", new
        {
            email = Input.Email,
            role = Input.Role
        });

        if (result.Success)
        {
            SetMessage("Invitation envoyée avec succès.");
            Input = new InviteInput();
        }

        return Page();
    }
}
