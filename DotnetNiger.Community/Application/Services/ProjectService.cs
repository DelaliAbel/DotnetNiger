using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.Notifications;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des projets communautaires : CRUD, mise en avant et notifications.</summary>
public class ProjectService(AppDbContext db, IServiceScopeFactory scopeFactory, ILogger<ProjectService> logger) : IProjectService
{
    /// <summary>Recherche paginée des projets, triés par mise en avant puis par date.</summary>
    public async Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page = 1, int pageSize = 10)
    {
        var q = db.Set<Project>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || p.Description.Contains(query) || p.Technologies.Contains(query));

        var total = await q.CountAsync();
        var projectEntities = await q
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var items = projectEntities.Select(MapProject).ToList();

        return new PaginatedResponse<ProjectResponse> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <summary>Projets mis en avant et publiés, du plus récent au plus ancien.</summary>
    public async Task<List<ProjectResponse>> GetFeaturedAsync()
    {
        var projects = await db.Set<Project>().AsNoTracking()
            .Where(p => p.IsFeatured && p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return projects.Select(MapProject).ToList();
    }

    /// <summary>Détail d'un projet.</summary>
    public async Task<ProjectResponse?> GetByIdAsync(Guid id)
    {
        var p = await db.Set<Project>().FindAsync(id);
        return p is null ? null : MapProject(p);
    }

    /// <summary>Détail d'un projet par son slug.</summary>
    public async Task<ProjectResponse?> GetBySlugAsync(string slug)
    {
        var p = await db.Set<Project>().AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug);
        return p is null ? null : MapProject(p);
    }

    /// <summary>Crée un projet et notifie les abonnés de la newsletter en arrière-plan.</summary>
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
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

        db.Add(project);
        await db.SaveChangesAsync();
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var notification = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try { await notification.NotifyNewProjectAsync(project.Title, project.Description, project.AuthorName); }
            catch (Exception ex) { logger.LogWarning(ex, "Échec de notification pour le nouveau projet {Title}", project.Title); }
        });
        return MapProject(project);
    }

    /// <summary>Modifie un projet (vérifie le propriétaire ou le rôle admin).</summary>
    public async Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin)
    {
        var p = await db.Set<Project>().FindAsync(id);
        if (p is null) return null;
        if (p.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Project.NotAuthorizedModify);

        p.Title = request.Title;
        p.Slug = GenerateSlug(request.Title);
        p.Description = request.Description;
        p.Url = request.Url;
        p.GithubUrl = request.GithubUrl;
        p.ImageUrl = request.ImageUrl;
        p.Technologies = request.Technologies;
        p.Status = request.Status;
        p.IsFeatured = request.IsFeatured;
        p.IsPublished = request.IsPublished;
        p.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapProject(p);
    }

    /// <summary>Suppression logique d'un projet.</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var p = await db.Set<Project>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return false;
        if (p.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Project.NotAuthorizedDelete);
        p.IsDeleted = true;
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    private static ProjectResponse MapProject(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Description = p.Description,
        Url = p.Url,
        GithubUrl = p.GithubUrl,
        ImageUrl = p.ImageUrl,
        Technologies = p.Technologies,
        Status = p.Status,
        CreatedBy = p.CreatedBy,
        AuthorName = p.AuthorName,
        IsFeatured = p.IsFeatured,
        IsPublished = p.IsPublished,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static string GenerateSlug(string text) => SlugGenerator.Generate(text);
}
