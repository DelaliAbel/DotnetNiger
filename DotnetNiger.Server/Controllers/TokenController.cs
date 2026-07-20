using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("Auth")]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;

    public TokenController(TokenService tokenService) => _tokenService = tokenService;

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> TokenExchange()
    {
        var result = await _tokenService.HandleTokenExchangeAsync(Request);
        if (result.RequiresTwoFactor)
            return Ok(new TwoFactorRequiredResponse(true, result.ChallengeToken!));
        if (result.Principal == null)
            return BadRequest(new { error = result.Error });
        return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _tokenService.SetupRefreshTokenContext(HttpContext, request.RefreshToken);
        return await TokenExchange();
    }
}
