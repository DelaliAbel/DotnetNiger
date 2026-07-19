using Asp.Versioning;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/api-keys")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService) => _apiKeyService = apiKeyService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ApiKeyResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var keys = await _apiKeyService.GetApiKeysAsync(new PaginationQuery(page, pageSize));
        return Ok(keys);
    }

    [HttpGet("{keyId:guid}")]
    public async Task<ActionResult<ApiKeyResponse>> GetById(Guid keyId)
    {
        var key = await _apiKeyService.GetApiKeyByIdAsync(keyId);
        return Ok(key);
    }

    [HttpPost]
    public async Task<ActionResult<ApiKeyCreatedResponse>> Create(
        [FromBody] CreateApiKeyRequest request)
    {
        var result = await _apiKeyService.CreateApiKeyAsync(request);
        return CreatedAtAction(nameof(GetById), new { keyId = result.Key.Id }, result);
    }

    [HttpPost("{keyId:guid}/rotate")]
    public async Task<ActionResult<ApiKeyCreatedResponse>> Rotate(Guid keyId)
    {
        var result = await _apiKeyService.RotateApiKeyAsync(keyId);
        return Ok(result);
    }

    [HttpDelete("{keyId:guid}")]
    public async Task<IActionResult> Delete(Guid keyId)
    {
        await _apiKeyService.DeleteApiKeyAsync(keyId);
        return NoContent();
    }
}
