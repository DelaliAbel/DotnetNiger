using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class PostService : IPostService
{
    private readonly IAuthService _authService;
    private List<PostDto> _posts;

    public PostService(IAuthService authService)
    {
        _authService = authService;
        _posts = new List<PostDto>
        {
            new PostDto
            {
                Id = Guid.NewGuid(),
                Title = "Les nouveautés de .NET 9",
                Slug = "les-nouveautes-de-dotnet-9",
                Excerpt = "Découvrez les dernières fonctionnalités et améliorations de .NET 9, avec C# 13 comme langage phare.",
                Content = "<h1>Introduction</h1><p>...</p>",
                CoverImageUrl = "/images/dotnet9.jpg",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Jean Dupont",
                AuthorAvatar = "/Images/ImageBlog.jpg",
                PostType = "Article",
                PublishedAt = DateTime.Now.AddDays(-5),
                ViewCount = 245,
                Categories = new List<CategoryDto>
                {
                    new CategoryDto { Id = Guid.NewGuid(), Name = "Technologie", Slug = "technologie", Description = "", PostCount = 10 }
                },
                Tags = new List<TagDto>
                {
                    new TagDto { Id = Guid.NewGuid(), Name = ".NET9", Slug = "dotnet9", UsageCount = 5 },
                    new TagDto { Id = Guid.NewGuid(), Name = "C#", Slug = "csharp", UsageCount = 15 }
                }
            },
            new PostDto
            {
                Id = Guid.NewGuid(),
                Title = "Introduction à Blazor WebAssembly",
                Slug = "introduction-a-blazor-webassembly",
                Excerpt = "Apprenez les bases de Blazor WASM...",
                Content = "<h1>Blazor WASM</h1><p>...</p>",
                CoverImageUrl = "/images/blazor.jpg",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Marie Martin",
                AuthorAvatar = "/images/avatars/marie.jpg",
                PostType = "Tutorial",
                PublishedAt = DateTime.Now.AddDays(-10),
                ViewCount = 512,
                Categories = new List<CategoryDto>
                {
                    new CategoryDto { Id = Guid.NewGuid(), Name = "Web", Slug = "web", Description = "", PostCount = 20 }
                },
                Tags = new List<TagDto>
                {
                    new TagDto { Id = Guid.NewGuid(), Name = "Blazor", Slug = "blazor", UsageCount = 8 },
                    new TagDto { Id = Guid.NewGuid(), Name = "WebAssembly", Slug = "webassembly", UsageCount = 6 }
                }
            }
        };
    }

    public async Task<List<PostDto>> GetAllPostsAsync()
    {
        await Task.Delay(2000);
        var posts = _posts
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return await Task.FromResult(posts);
    }
  
    public async Task<List<PostDto>> GetPublishedPostsAsync()
    {
        await Task.Delay(2000);
        var posts = _posts
            .Where(p => p.PublishedAt != DateTime.MinValue)
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return await Task.FromResult(posts);
    }

    public async Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug)
    {
        await Task.Delay(800);
        var posts = _posts
            .Where(p => p.Categories.Any(c => c.Slug == categorySlug))
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return await Task.FromResult(posts);
    }

    public async Task<List<PostDto>> GetPostsByTagAsync(string tagSlug)
    {
        await Task.Delay(2000);
        var posts = _posts
            .Where(p => p.Tags.Any(t => t.Slug == tagSlug))
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return await Task.FromResult(posts);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        await Task.Delay(800);
        var post = _posts.FirstOrDefault(p => p.Id == id);

        if (post == null)
            return await Task.FromResult<PostDto?>(null);

        return await Task.FromResult<PostDto?>(post);
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug)
    {
        await Task.Delay(2000);
        var post = _posts.FirstOrDefault(p => p.Slug == slug);

        if (post == null)
            return await Task.FromResult<PostDto?>(null);

        return await Task.FromResult<PostDto?>(post);
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post is not null) post.ViewCount++;
        await Task.CompletedTask;
    }

    public async Task<List<PostDto>> SearchPostsAsync(string query)
    {
        await Task.Delay(800);
        var posts = _posts
            .Where(p =>
                p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Excerpt.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.AuthorName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return await Task.FromResult(posts);
    }

    public async Task<List<PostDto>> GetMyPostsAsync()
    {
        await Task.Delay(800);
        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return new();
        return _posts.Where(p => p.AuthorId == user.Id).OrderByDescending(p => p.PublishedAt).ToList();
    }

    public Task<List<PostDto>> GetAdminPostsAsync(string? status = null)
    {
        var posts = _posts.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
            posts = posts.Where(p =>
                (status == "published" && p.PublishedAt != default) ||
                (status == "draft" && p.PublishedAt == default));
        return Task.FromResult(posts.ToList());
    }
}
