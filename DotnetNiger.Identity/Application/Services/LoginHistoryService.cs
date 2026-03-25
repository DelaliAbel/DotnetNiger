// Service applicatif Identity: LoginHistoryService
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Services;

// Service de journalisation des connexions.
public class LoginHistoryService : ILoginHistoryService
{
	private readonly ILoginHistoryRepository _loginHistoryRepository;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public LoginHistoryService(
		ILoginHistoryRepository loginHistoryRepository,
		IHttpContextAccessor httpContextAccessor)
	{
		_loginHistoryRepository = loginHistoryRepository;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task RecordAsync(Guid userId, bool success, string? failureReason, CancellationToken ct = default)
	{
		var context = _httpContextAccessor.HttpContext;
		var ip = context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
		var userAgent = context?.Request.Headers.UserAgent.ToString() ?? string.Empty;

		var history = new LoginHistory
		{
			UserId = userId,
			Success = success,
			FailureReason = failureReason ?? string.Empty,
			IpAddress = ip,
			UserAgent = userAgent,
			// TODO: Implementer la geolocalisation IP (ex: MaxMind GeoIP2) pour remplir Country et City.
			Country = string.Empty,
			City = string.Empty
		};

		await _loginHistoryRepository.AddAsync(history);
	}

	public async Task<PaginatedDto<LoginHistoryDto>> GetUserHistoryAsync(Guid userId, int skip, int take, CancellationToken ct = default)
	{
		var query = _loginHistoryRepository.Query()
			.AsNoTracking()
			.Where(history => history.UserId == userId);

		var total = await query.CountAsync(ct);
		var items = await query
			.OrderByDescending(history => history.LoginAt)
			.Skip(skip)
			.Take(take)
			.Select(history => new LoginHistoryDto
			{
				Id = history.Id,
				LoginAt = history.LoginAt,
				IpAddress = history.IpAddress,
				UserAgent = history.UserAgent,
				Success = history.Success,
				FailureReason = history.FailureReason,
				Country = history.Country,
				City = history.City
			})
			.ToListAsync(ct);

		return new PaginatedDto<LoginHistoryDto>
		{
			Items = items,
			TotalCount = total,
			Skip = skip,
			Take = take
		};
	}
}
