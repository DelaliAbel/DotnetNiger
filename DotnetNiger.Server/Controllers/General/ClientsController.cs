using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers.General;

[ApiController]
[Route("api/admin/clients")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class ClientsController : ControllerBase
{
    private readonly OAuthClientService _clientService;
    private readonly OpenIddictClientManager _clientManager;

    public ClientsController(OAuthClientService clientService, OpenIddictClientManager clientManager)
    {
        _clientService = clientService;
        _clientManager = clientManager;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<OAuthClientResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var clients = await _clientService.GetClientsAsync(new PaginationQuery(page, pageSize));
        return Ok(clients);
    }

    [HttpGet("{clientId:guid}")]
    public async Task<ActionResult<OAuthClientResponse>> GetById(Guid clientId)
    {
        var client = await _clientService.GetClientByIdAsync(clientId);
        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<OAuthClientCreatedResponse>> Create(
        [FromBody] CreateOAuthClientRequest request)
    {
        var result = await _clientManager.CreateClientAsync(request);
        return CreatedAtAction(nameof(GetById), new { clientId = result.Client.Id }, result);
    }

    [HttpPut("{clientId:guid}")]
    public async Task<ActionResult<OAuthClientResponse>> Update(Guid clientId,
        [FromBody] UpdateOAuthClientRequest request)
    {
        var client = await _clientManager.UpdateClientAsync(clientId, request);
        return Ok(client);
    }

    [HttpDelete("{clientId:guid}")]
    public async Task<IActionResult> Delete(Guid clientId)
    {
        await _clientManager.DeleteClientAsync(clientId);
        return NoContent();
    }
}
