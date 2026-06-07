using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class DocsModel : PageModel
{
    public string Lang { get; set; } = "fr";
    public string GatewayUrl { get; private set; } = "http://localhost:5000";
    public string IdentityUrl { get; private set; } = "http://localhost:5075";
    public string SupportReportUrl { get; private set; } = "";

    public void OnGet(string? lang)
    {
        Lang = lang switch
        {
            "en" => "en",
            _ => "fr"
        };
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        GatewayUrl = (config["DeveloperPortal:GatewayBaseUrl"] ?? "http://localhost:5000").TrimEnd('/');
        IdentityUrl = (config["Identity:BaseUrl"] ?? "http://localhost:5075").TrimEnd('/');
        SupportReportUrl = $"{IdentityUrl}/api/v1/support/report";
    }
}
