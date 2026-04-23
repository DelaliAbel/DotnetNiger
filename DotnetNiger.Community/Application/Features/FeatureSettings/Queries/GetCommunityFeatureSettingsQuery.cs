using DotnetNiger.Community.Application.DTOs.Responses;
using MediatR;

namespace DotnetNiger.Community.Application.Features.FeatureSettings.Queries;

public sealed record GetCommunityFeatureSettingsQuery : IRequest<CommunityFeatureSettingsResponse>;
