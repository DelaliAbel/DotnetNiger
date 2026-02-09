using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TokensController : ControllerBase
{
	private readonly ITokenService _tokenService;

	public TokensController(ITokenService tokenService)
	{
		_tokenService = tokenService;
	}

	[HttpPost("refresh")]
	public async Task<ActionResult<AuthDto>> Refresh([FromBody] RefreshTokenRequest request)
	{
		var result = await _tokenService.RefreshAsync(request);
		return Ok(result);
	}
}
