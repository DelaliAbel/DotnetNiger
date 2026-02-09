using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tokens")]
[Authorize]
public class TokensController : ControllerBase
{
	// Endpoints proteges pour la gestion des tokens.
	private readonly ITokenService _tokenService;

	public TokensController(ITokenService tokenService)
	{
		_tokenService = tokenService;
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthDto>> Refresh([FromBody] RefreshTokenRequest request)
	{
		var result = await _tokenService.RefreshAsync(request);
		return Ok(result);
	}
}
