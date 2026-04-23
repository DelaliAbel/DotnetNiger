using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Features.FeatureSettings.Queries;
using DotnetNiger.Community.Infrastructure.Caching;
using MediatR;

namespace DotnetNiger.Community.Application.Features.FeatureSettings.Commands;

public sealed class UpdateCommunityFeatureSettingsCommandHandler : IRequestHandler<UpdateCommunityFeatureSettingsCommand, CommunityFeatureSettingsResponse>
{
    private readonly IAppSettingPersistence _appSettingPersistence;
    private readonly ICacheService _cacheService;
    private readonly ISender _sender;

    public UpdateCommunityFeatureSettingsCommandHandler(IAppSettingPersistence appSettingPersistence, ICacheService cacheService, ISender sender)
    {
        _appSettingPersistence = appSettingPersistence;
        _cacheService = cacheService;
        _sender = sender;
    }

    public async Task<CommunityFeatureSettingsResponse> Handle(UpdateCommunityFeatureSettingsCommand command, CancellationToken cancellationToken)
    {
        if (command.Request.PostsEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.PostsEnabled, command.Request.PostsEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.CommentsEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.CommentsEnabled, command.Request.CommentsEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.EventsEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.EventsEnabled, command.Request.EventsEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.ProjectsEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.ProjectsEnabled, command.Request.ProjectsEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.ResourcesEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.ResourcesEnabled, command.Request.ResourcesEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.SearchEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.SearchEnabled, command.Request.SearchEnabled.Value.ToString(), command.UpdatedByUserId);

        if (command.Request.PublicAccessEnabled.HasValue)
            await _appSettingPersistence.SetValueAsync(FeatureSettingKeys.PublicAccessEnabled, command.Request.PublicAccessEnabled.Value.ToString(), command.UpdatedByUserId);

        await _cacheService.RemoveAsync(FeatureSettingsCacheKey.Value, cancellationToken);
        return await _sender.Send(new GetCommunityFeatureSettingsQuery(), cancellationToken);
    }
}
