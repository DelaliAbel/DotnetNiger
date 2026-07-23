using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Community;

/// <summary>Service de gestion des projets communautaires.</summary>
public class ProjectService : IProjectService
{
    private readonly DotnetNigerDbContext _db;

    public ProjectService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste paginée des projets avec filtres.</summary>
    public async Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page, int pageSize)
    {
        var q = _db.Set<Project>().AsNoTracking().Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || p.Description.Contains(query));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ProjectResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Récupère les projets mis en avant.</summary>
    public async Task<List<ProjectResponse>> GetFeaturedAsync()
    {
        return await _db.Set<Project>().AsNoTracking()
            .Where(p => p.IsFeatured && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToResponse(p))
            .ToListAsync();
    }

    /// <summary>Récupère un projet par identifiant.</summary>
    public async Task<ProjectResponse?> GetByIdAsync(Guid id)
    {
        var p = await _db.Set<Project>().FindAsync(id);
        return p == null || p.IsDeleted ? null : MapToResponse(p);
    }

    /// <summary>Récupère un projet par slug.</summary>
    public async Task<ProjectResponse?> GetBySlugAsync(string slug)
    {
        var p = await _db.Set<Project>().FirstOrDefaultAsync(pr => pr.Slug == slug && !pr.IsDeleted);
        return p == null ? null : MapToResponse(p);
    }

    /// <summary>Crée un nouveau projet.</summary>
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Title.ToLower().Replace(" ", "-"),
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
        await _db.SaveChangesAsync();
        return MapToResponse(project);
    }

    /// <summary>Met à jour un projet existant.</summary>
    public async Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin)
    {
        var project = await _db.Set<Project>().FindAsync(id);
        if (project == null) return null;
        if (project.IsDeleted) return null;

        if (!isAdmin && project.CreatedBy != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier ce projet.");

        if (request.Title != null)
        {
            project.Title = request.Title;
            project.Slug = GenerateSlug(request.Title);
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
        await _db.SaveChangesAsync();
        return MapToResponse(project);
    }

    /// <summary>Supprime un projet (suppression logique).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var project = await _db.Set<Project>().FindAsync(id);
        if (project == null) return false;
        if (!isAdmin && project.CreatedBy != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer ce projet.");
        project.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static string GenerateSlug(string title) =>
        System.Text.RegularExpressions.Regex.Replace(
            title.ToLower().Trim(),
            @"[^a-z0-9\s-]",
            ""
        ).Replace(" ", "-")
         .Replace("--", "-")
         .Trim('-');

    private static ProjectResponse MapToResponse(Project p) => new()
    {
        Id = p.Id, Title = p.Title, Slug = p.Slug, Description = p.Description,
        Url = p.Url, GithubUrl = p.GithubUrl, ImageUrl = p.ImageUrl,
        Technologies = p.Technologies, Status = p.Status, CreatedBy = p.CreatedBy,
        AuthorName = p.AuthorName, IsFeatured = p.IsFeatured, IsPublished = p.IsPublished,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}
