using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TestController : ApiControllerBase
{
    private readonly IIdentityApiClient _identityApiClient;

    public TestController(IIdentityApiClient identityApiClient)
    {
        _identityApiClient = identityApiClient;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Success(new
        {
            status = "ok",
            service = "DotnetNiger.Community",
            utcTime = DateTime.UtcNow
        });
    }

    [HttpGet("identity-health")]
    public async Task<IActionResult> GetIdentityHealth(CancellationToken cancellationToken)
    {
        var reachable = await _identityApiClient.IsReachableAsync(cancellationToken);
        return Success(new IdentityConnectivityResponse
        {
            Reachable = reachable,
            BaseUrl = _identityApiClient.BaseUrl,
            CheckedAtUtc = DateTime.UtcNow
        });
    }
}
