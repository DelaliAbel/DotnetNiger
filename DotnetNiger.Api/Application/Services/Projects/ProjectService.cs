using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Projects;

/// <summary>Service de gestion des projets communautaires.</summary>
public class ProjectService : IProjectService
{
    private readonly DotnetNigerDbContext _db;

    public ProjectService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste paginée des projets avec filtres.</summary>
    public async Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page, int pageSize, Guid? createdBy = null, CancellationToken ct = default)
    {
        var q = _db.Set<Project>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || p.Description.Contains(query));
        if (createdBy.HasValue)
            q = q.Where(p => p.CreatedBy == createdBy.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<ProjectResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Récupère les projets mis en avant.</summary>
    public async Task<List<ProjectResponse>> GetFeaturedAsync(CancellationToken ct = default)
    {
        return await _db.Set<Project>().AsNoTracking()
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);
    }

    /// <summary>Récupère un projet par identifiant.</summary>
    public async Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Set<Project>().FindAsync(id, ct);
        return p == null ? null : MapToResponse(p);
    }

    /// <summary>Récupère un projet par slug.</summary>
    public async Task<ProjectResponse?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var p = await _db.Set<Project>().FirstOrDefaultAsync(pr => pr.Slug == slug, ct);
        return p == null ? null : MapToResponse(p);
    }

    /// <summary>Crée un nouveau projet.</summary>
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName, CancellationToken ct = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = await GenerateUniqueSlug(null, request.Title, ct),
            Description = request.Description,
            Url = request.Url,
            GithubUrl = request.GithubUrl,
            ImageUrl = request.ImageUrl,
            Technologies = request.Technologies,
            Status = request.Status,
            CreatedBy = userId,
            AuthorName = authorName,
            IsFeatured = request.IsFeatured,
            IsPublished = request.IsPublished
        };
        _db.Set<Project>().Add(project);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(project);
    }

    /// <summary>Met à jour un projet existant.</summary>
    public async Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var project = await _db.Set<Project>().FindAsync(id, ct);
        if (project == null) return null;

        if (!isAdmin && project.CreatedBy != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier ce projet.");

        if (request.Title != null)
        {
            project.Title = request.Title;
            project.Slug = await GenerateUniqueSlug(null, request.Title, ct);
        }
        if (request.Description != null) project.Description = request.Description;
        if (request.Url != null) project.Url = request.Url;
        if (request.GithubUrl != null) project.GithubUrl = request.GithubUrl;
        if (request.ImageUrl != null) project.ImageUrl = request.ImageUrl;
        if (request.Technologies != null) project.Technologies = request.Technologies;
        if (request.Status != null) project.Status = request.Status;
        if (request.IsFeatured.HasValue) project.IsFeatured = request.IsFeatured.Value;
        if (request.IsPublished.HasValue) project.IsPublished = request.IsPublished.Value;

        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapToResponse(project);
    }

    /// <summary>Supprime un projet (suppression définitive).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var project = await _db.Set<Project>().FindAsync(id, ct);
        if (project == null) return false;
        if (!isAdmin && project.CreatedBy != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer ce projet.");
        _db.Set<Project>().Remove(project);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string title, CancellationToken ct = default)
    {
        var baseSlug = !string.IsNullOrWhiteSpace(providedSlug)
            ? providedSlug
            : title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
                .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "projet";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Set<Project>().AnyAsync(p => p.Slug == candidate, ct))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid entityId, CancellationToken ct = default)
    {
        var candidate = slug;
        var suffix = 1;
        while (await _db.Set<Project>().AnyAsync(p => p.Slug == candidate && p.Id != entityId, ct))
        {
            candidate = $"{slug}-{suffix++}";
        }
        return candidate;
    }

    private static ProjectResponse MapToResponse(Project p) => new()
    {
        Id = p.Id, Title = p.Title, Slug = p.Slug, Description = p.Description,
        Url = p.Url, GithubUrl = p.GithubUrl, ImageUrl = p.ImageUrl,
        Technologies = p.Technologies, Status = p.Status, CreatedBy = p.CreatedBy,
        AuthorName = p.AuthorName, IsFeatured = p.IsFeatured, IsPublished = p.IsPublished,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}
