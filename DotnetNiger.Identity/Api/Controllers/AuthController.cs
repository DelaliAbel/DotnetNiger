using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthDto>> Register([FromBody] RegisterRequest request)
	{
		var result = await _authService.RegisterAsync(request);
		return Ok(result);
	}

	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthDto>> Login([FromBody] LoginRequest request)
	{
		var result = await _authService.LoginAsync(request);
		return Ok(result);
	}
}
