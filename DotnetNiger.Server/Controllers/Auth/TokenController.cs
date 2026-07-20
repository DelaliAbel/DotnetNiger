using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Server.Controllers.Auth;

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
