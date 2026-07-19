using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Mappers statiques pour les ressources pédagogiques.</summary>
internal static class ResourceMappers
{
    /// <summary>Transforme une Resource en ResourceResponse.</summary>
    public static ResourceResponse ToResponse(Resource r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Slug = r.Slug,
        Description = r.Description,
        Url = r.Url,
        ResourceType = r.ResourceType,
        Level = r.Level,
        CreatedBy = r.CreatedBy,
        ViewCount = r.ViewCount,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        CategoryIds = r.ResourceCategories.Select(rc => rc.CategoryId).ToList(),
        Tags = r.ResourceTags.Select(rt => new TagResponse
        {
            Id = rt.Tag.Id,
            Name = rt.Tag.Name,
            Slug = rt.Tag.Slug,
            UsageCount = rt.Tag.UsageCount
        }).ToList()
    };
}
