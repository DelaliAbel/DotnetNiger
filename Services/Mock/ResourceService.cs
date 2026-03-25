using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock;

public class ResourceService : IResourceService
{
    private List<ResourceDto> _resources;

    public ResourceService()
    {
        _resources = new List<ResourceDto>
        {
            new ResourceDto
            {
                Id = Guid.NewGuid(),
                Title = "Documentation officielle .NET 8",
                Slug = "documentation-officielle-dotnet-8",
                Description = "La documentation complète de .NET 8 par Microsoft : guides, tutoriels et référence API.",
                Url = "https://learn.microsoft.com/fr-fr/dotnet/",
                ResourceType = "Documentation",
                Level = "Tous niveaux",
                ViewCount = 320
            },
            new ResourceDto
            {
                Id = Guid.NewGuid(),
                Title = "Tutoriel Blazor WebAssembly — Getting Started",
                Slug = "tutoriel-blazor-webassembly-getting-started",
                Description = "Guide de démarrage officiel Microsoft pour créer votre première application Blazor WASM.",
                Url = "https://learn.microsoft.com/fr-fr/aspnet/core/blazor/",
                ResourceType = "Tutoriel",
                Level = "Débutant",
                ViewCount = 214
            },
            new ResourceDto
            {
                Id = Guid.NewGuid(),
                Title = "Clean Architecture avec ASP.NET Core",
                Slug = "clean-architecture-aspnet-core",
                Description = "Implémentation de la Clean Architecture dans un projet ASP.NET Core : séparation des couches, DI, CQRS.",
                Url = "https://github.com/ardalis/CleanArchitecture",
                ResourceType = "GitHub",
                Level = "Intermédiaire",
                ViewCount = 178
            },
            new ResourceDto
            {
                Id = Guid.NewGuid(),
                Title = "Design Patterns en C# — Refactoring.Guru",
                Slug = "design-patterns-csharp-refactoring-guru",
                Description = "Catalogue complet des design patterns GoF avec exemples en C# et diagrammes UML.",
                Url = "https://refactoring.guru/fr/design-patterns/csharp",
                ResourceType = "Article",
                Level = "Avancé",
                ViewCount = 95
            },
            new ResourceDto
            {
                Id = Guid.NewGuid(),
                Title = "Entity Framework Core — Guide complet",
                Slug = "entity-framework-core-guide-complet",
                Description = "Tout ce qu'il faut savoir sur EF Core : migrations, requêtes LINQ, relations, performance.",
                Url = "https://learn.microsoft.com/fr-fr/ef/core/",
                ResourceType = "Documentation",
                Level = "Intermédiaire",
                ViewCount = 267
            }
        };
    }

    // ── Lecture ────────────────────────────────────────────────

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        return await Task.FromResult(
            _resources.OrderByDescending(r => r.ViewCount).ToList());
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(Guid id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        return await Task.FromResult(resource);
    }

    public async Task<ResourceDto?> GetResourceBySlugAsync(string slug)
    {
        var resource = _resources.FirstOrDefault(r => r.Slug == slug);
        if (resource is not null) resource.ViewCount++;
        return await Task.FromResult(resource);
    }

    public async Task<List<ResourceDto>> GetResourcesByTypeAsync(string resourceType)
    {
        return await Task.FromResult(
            _resources.Where(r => r.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase))
                      .OrderByDescending(r => r.ViewCount)
                      .ToList());
    }

    public async Task<List<ResourceDto>> GetResourcesByLevelAsync(string level)
    {
        return await Task.FromResult(
            _resources.Where(r => r.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
                      .OrderByDescending(r => r.ViewCount)
                      .ToList());
    }

    public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
    {
        return await Task.FromResult(
            _resources.Where(r =>
                    r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.ResourceType.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.ViewCount)
                .ToList());
    }

    public async Task<List<string>> GetResourceTypesAsync()
    {
        return await Task.FromResult(
            _resources.Select(r => r.ResourceType).Distinct().OrderBy(t => t).ToList());
    }

    public async Task<List<string>> GetLevelsAsync()
    {
        return await Task.FromResult(
            _resources.Select(r => r.Level).Distinct().ToList());
    }

    // ── Création / Mise à jour / Suppression ───────────────────

    public async Task<ResourceDto> CreateResourceAsync(CreateResourceRequest request)
    {
        var newResource = new ResourceDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Level = request.Level,
            ViewCount = 0
        };

        _resources.Add(newResource);
        return await Task.FromResult(newResource);
    }

    public async Task<ResourceDto> AddResourceAsync(AddResourceRequest request)
    {
        var newResource = new ResourceDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Level = request.Level,
            ViewCount = 0
        };

        _resources.Add(newResource);
        return await Task.FromResult(newResource);
    }

    public async Task<ResourceDto?> UpdateResourceAsync(Guid id, CreateResourceRequest request)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null) return await Task.FromResult<ResourceDto?>(null);

        resource.Title = request.Title;
        resource.Slug = GenerateSlug(request.Title);
        resource.Description = request.Description;
        resource.Url = request.Url;
        resource.ResourceType = request.ResourceType;
        resource.Level = request.Level;

        return await Task.FromResult<ResourceDto?>(resource);
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null) return await Task.FromResult(false);

        _resources.Remove(resource);
        return await Task.FromResult(true);
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is not null) resource.ViewCount++;
        await Task.CompletedTask;
    }

    // ── Utilitaires ────────────────────────────────────────────

    private static string GenerateSlug(string title)
    {
        return title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("à", "a").Replace("â", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o")
            .Replace("ù", "u").Replace("û", "u")
            .Replace("ç", "c")
            .Replace("'", "-").Replace("\"", "")
            .Replace(",", "").Replace(".", "")
            .Replace("?", "").Replace("!", "").Replace("#", "");
    }
}
