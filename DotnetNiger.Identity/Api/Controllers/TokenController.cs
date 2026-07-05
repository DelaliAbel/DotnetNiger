using Asp.Versioning;
using DotnetNiger.Common.Auth.Requests;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
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
        HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = request.RefreshToken,
            ["client_id"] = "web-ui",
            ["scope"] = "openid profile email roles offline_access"
        });
        return await TokenExchange();
    }
}
