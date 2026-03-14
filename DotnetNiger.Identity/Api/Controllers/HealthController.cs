using Asp.Versioning;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class HealthController : ApiControllerBase
{
	private readonly DotnetNigerIdentityDbContext _dbContext;

	public HealthController(DotnetNigerIdentityDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	[HttpGet("health")]
	[AllowAnonymous]
	public IActionResult Health()
	{
		return Success(new { status = "healthy", service = "DotnetNiger.Identity", timestamp = DateTime.UtcNow });
	}

	[HttpGet("health/detailed")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> DetailedHealth()
	{
		var dbOk = await _dbContext.Database.CanConnectAsync();
		return Success(new
		{
			status = dbOk ? "healthy" : "degraded",
			db = dbOk ? "ok" : "unreachable",
			cache = "n/a",
			auth = "ok",
			version = "1.0",
			timestamp = DateTime.UtcNow
		});
	}
}
