using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using MediatR;

namespace DotnetNiger.Community.Application.Features.FeatureSettings.Commands;

public sealed record UpdateCommunityFeatureSettingsCommand(UpdateCommunityFeatureSettingsRequest Request, Guid? UpdatedByUserId) : IRequest<CommunityFeatureSettingsResponse>;
