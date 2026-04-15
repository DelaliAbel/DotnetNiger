using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test")]
public class TestController : ApiControllerBase
{
    private readonly IIdentityApiClient _identityApiClient;

    private readonly CommunityDbContext _dbContext;


    public TestController(IIdentityApiClient identityApiClient, CommunityDbContext dbContext)
    {
        _identityApiClient = identityApiClient;
        _dbContext = dbContext;
    }

    [HttpGet("health/detailed")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DetailedHealth()
    {
        var dbOk = await _dbContext.Database.CanConnectAsync();
        return Success(new
        {
            status = dbOk ? "Healthy" : "Degraded",
            db = dbOk ? "Ok" : "Unreachable",
            cache = "n/a",
            auth = "Ok",
            version = "1.0",
            timestamp = DateTime.UtcNow
        });
    }
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Success(new
        {
            status = "Ok",
            service = "DotnetNiger.Community",
            utcTime = DateTime.UtcNow
        });
    }

    [HttpGet("check-identity")]
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
