using Asp.Versioning;
using DotnetNiger.Common.Constants;
using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Application;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("Auth")]
public class AdminAuthController : ControllerBase
{
    private readonly TenantInitializationService _tenantInitService;
    private readonly OpenIddictManagementService _openIddictManagement;
    private readonly SmtpOptions _smtp;

    public AdminAuthController(
        TenantInitializationService tenantInitService,
        OpenIddictManagementService openIddictManagement,
        IOptions<SmtpOptions> smtp)
    {
        _tenantInitService = tenantInitService;
        _openIddictManagement = openIddictManagement;
        _smtp = smtp.Value;
    }

    /// <summary>Enregistre un nouveau locataire (tenant) dans le système.</summary>
    /// <param name="request">Requête contenant les informations du tenant.</param>
    [HttpPost("register-tenant")]
    [EnableRateLimiting("TenantRegistration")]
    public async Task<ActionResult<RegisterTenantResponse>> RegisterTenant([FromBody] RegisterTenantRequest request)
    {
        var result = await _tenantInitService.RegisterTenantAsync(request);
        return Ok(result);
    }

    /// <summary>Initialise les applications OpenIddict pour l'interface web.</summary>
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    [HttpPost("bootstrap-web-ui")]
    public async Task<IActionResult> BootstrapWebUi([FromServices] IOpenIddictApplicationManager appManager)
    {
        var message = await _openIddictManagement.BootstrapWebUiAsync(appManager, _smtp.FrontendBaseUrl);
        return Ok(new { message });
    }
}
