using System.Security.Claims;
using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
{
	// Endpoints proteges pour le profil utilisateur.
	private readonly IUserService _userService;

	public UsersController(IUserService userService)
	{
		_userService = userService;
	}

	[HttpGet("me")]
	public async Task<ActionResult<UserDto>> Me()
	{
		var userId = GetUserId();
		if (userId == null)
		{
			return Unauthorized();
		}

		var profile = await _userService.GetProfileAsync(userId.Value);
		return Ok(profile);
	}

	[HttpPut("me")]
	public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest request)
	{
		var userId = GetUserId();
		if (userId == null)
		{
			return Unauthorized();
		}

		var profile = await _userService.UpdateProfileAsync(userId.Value, request);
		return Ok(profile);
	}

	[HttpPost("me/change-password")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		var userId = GetUserId();
		if (userId == null)
		{
			return Unauthorized();
		}

		await _userService.ChangePasswordAsync(userId.Value, request);
		return NoContent();
	}

	[HttpPost("me/change-email")]
	public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
	{
		var userId = GetUserId();
		if (userId == null)
		{
			return Unauthorized();
		}

		await _userService.ChangeEmailAsync(userId.Value, request);
		return NoContent();
	}

	private Guid? GetUserId()
	{
		var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrWhiteSpace(userIdValue))
		{
			return null;
		}

		return Guid.TryParse(userIdValue, out var userId) ? userId : null;
	}
}
