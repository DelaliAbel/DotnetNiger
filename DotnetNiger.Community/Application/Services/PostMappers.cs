using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Mappers statiques pour les articles de blog.</summary>
internal static class PostMappers
{
    /// <summary>Transforme un Post en PostResponse.</summary>
    public static PostResponse ToResponse(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Content = p.Content,
        Excerpt = p.Excerpt,
        CoverImageUrl = p.CoverImageUrl,
        AuthorId = p.AuthorId,
        AuthorName = p.AuthorName,
        AuthorAvatar = p.AuthorAvatar,
        PostType = p.PostType,
        IsPublished = p.IsPublished,
        PublishedAt = p.PublishedAt ?? DateTime.MinValue,
        ViewCount = p.ViewCount,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Categories = p.PostCategories.Select(pc => new CategoryResponse
        {
            Id = pc.Category.Id,
            Name = pc.Category.Name,
            Slug = pc.Category.Slug,
            Description = pc.Category.Description,
            PostCount = pc.Category.PostCount
        }).ToList(),
        Tags = p.PostTags.Select(pt => new TagResponse
        {
            Id = pt.Tag.Id,
            Name = pt.Tag.Name,
            Slug = pt.Tag.Slug,
            UsageCount = pt.Tag.UsageCount
        }).ToList()
    };
}
