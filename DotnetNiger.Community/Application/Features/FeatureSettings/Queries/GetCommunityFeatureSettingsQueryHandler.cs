using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Features.FeatureSettings;
using DotnetNiger.Community.Infrastructure.Caching;
using MediatR;

namespace DotnetNiger.Community.Application.Features.FeatureSettings.Queries;

public sealed class GetCommunityFeatureSettingsQueryHandler : IRequestHandler<GetCommunityFeatureSettingsQuery, CommunityFeatureSettingsResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IAppSettingPersistence _appSettingPersistence;
    private readonly ICacheService _cacheService;

    public GetCommunityFeatureSettingsQueryHandler(IAppSettingPersistence appSettingPersistence, ICacheService cacheService)
    {
        _appSettingPersistence = appSettingPersistence;
        _cacheService = cacheService;
    }

    public async Task<CommunityFeatureSettingsResponse> Handle(GetCommunityFeatureSettingsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<CommunityFeatureSettingsResponse>(FeatureSettingsCacheKey.Value, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var response = new CommunityFeatureSettingsResponse
        {
            PostsEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.PostsEnabled, true),
            CommentsEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.CommentsEnabled, true),
            EventsEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.EventsEnabled, true),
            ProjectsEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.ProjectsEnabled, true),
            ResourcesEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.ResourcesEnabled, true),
            SearchEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.SearchEnabled, true),
            PublicAccessEnabled = await GetBooleanSettingAsync(FeatureSettingKeys.PublicAccessEnabled, true),
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _cacheService.SetAsync(FeatureSettingsCacheKey.Value, response, CacheDuration, cancellationToken);
        return response;
    }

    private async Task<bool> GetBooleanSettingAsync(string key, bool fallback)
    {
        var value = await _appSettingPersistence.GetValueAsync(key);
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
