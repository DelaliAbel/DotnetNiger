using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using DotnetNiger.Infrastructure.Auth;
using OpenIddict.Validation.AspNetCore;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/external-services")]
public class ExternalServicesController : ControllerBase
{
    private readonly IExternalServiceService _service;

    public ExternalServicesController(IExternalServiceService service)
    {
        _service = service;
    }

    private const string BothSchemes = "ApiKey," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

    [HttpPost("register")]
    [Authorize(AuthenticationSchemes = BothSchemes)]
    public async Task<ActionResult<ExternalServiceResponse>> Register(
        [FromBody] RegisterExternalServiceRequest request)
    {
        try
        {
            var result = await _service.RegisterAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = BothSchemes)]
    public async Task<ActionResult<PaginatedResponse<ExternalServiceResponse>>> GetMyServices(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var services = await _service.GetAllAsync(new PaginationQuery(page, pageSize));
        return Ok(services);
    }

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = BothSchemes)]
    public async Task<ActionResult<ExternalServiceResponse>> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(AuthenticationSchemes = BothSchemes)]
    public async Task<ActionResult<ExternalServiceResponse>> Update(
        Guid id, [FromBody] UpdateExternalServiceRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = BothSchemes)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [InternalApiKeyAuth]
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ServiceLookupResult>> ResolveSlug(string slug)
    {
        var result = await _service.ResolveSlugAsync(slug);
        if (result == null)
            return NotFound(new { error = "Service not found or not active" });
        return Ok(result);
    }

    [HttpGet("_internal/active")]
    [InternalApiKeyAuth]
    public async Task<ActionResult<List<ExternalServiceResponse>>> GetAllActive()
    {
        var services = await _service.GetAllActiveAsync();
        return Ok(services);
    }

    [HttpPost("_internal/{id:guid}/health-result")]
    [InternalApiKeyAuth]
    public async Task<IActionResult> ReportHealthResult(Guid id, [FromBody] HealthCheckResultResponse result)
    {
        await _service.UpdateHealthStatusAsync(id, result.IsHealthy);
        return Ok();
    }
}
