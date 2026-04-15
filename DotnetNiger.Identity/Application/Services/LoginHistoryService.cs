// Service applicatif Identity: LoginHistoryService
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Services;

// Service de journalisation des connexions avec geolocalisation IP.
public class LoginHistoryService : ILoginHistoryService
{
    private readonly ILoginHistoryPersistence _loginHistoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGeoLocationProvider _geoLocationProvider;

    public LoginHistoryService(
        ILoginHistoryPersistence loginHistoryRepository,
        IHttpContextAccessor httpContextAccessor,
        IGeoLocationProvider geoLocationProvider)
    {
        _loginHistoryRepository = loginHistoryRepository;
        _httpContextAccessor = httpContextAccessor;
        _geoLocationProvider = geoLocationProvider;
    }

    public async Task RecordAsync(Guid userId, bool success, string? failureReason, CancellationToken ct = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var ip = context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var userAgent = context?.Request.Headers.UserAgent.ToString() ?? string.Empty;

        // Get geolocation data from IP address
        var geoLocation = await _geoLocationProvider.GetAsync(ip, ct);

        var history = new LoginHistory
        {
            UserId = userId,
            Success = success,
            FailureReason = failureReason ?? string.Empty,
            IpAddress = ip,
            UserAgent = userAgent,
            Country = geoLocation.Country,
            City = geoLocation.City
        };

        await _loginHistoryRepository.AddAsync(history);
    }

    public async Task<PaginatedResponse<LoginHistoryResponse>> GetUserHistoryAsync(Guid userId, int skip, int take, CancellationToken ct = default)
    {
        var query = _loginHistoryRepository.Query()
            .AsNoTracking()
            .Where(history => history.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(history => history.LoginAt)
            .Skip(skip)
            .Take(take)
            .Select(history => new LoginHistoryResponse
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

        return new PaginatedResponse<LoginHistoryResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }
}
