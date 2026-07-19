using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class PostService
{
    public async Task<PostDto?> CreatePostAsync(CreatePostRequest request, Guid CurrentId)
    {
        var newPost = new PostDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Excerpt = request.Excerpt,
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl ?? "/images/default.jpg",
            AuthorId = CurrentId, // à remplacer par l'utilisateur connecté
            AuthorName = "Admin",
            AuthorAvatar = "/images/avatars/default.jpg",
            PostType = request.PostType,
            PublishedAt = DateTime.Now,
            ViewCount = 0,
            Categories = new List<CategoryDto>(),
            Tags = new List<TagDto>(),
        };

        _posts.Add(newPost);

        return await Task.FromResult(newPost);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
            return await Task.FromResult(false);

        _posts.Remove(post);
        return await Task.FromResult(true);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostRequest request)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);

        if (post == null)
            return await Task.FromResult<PostDto?>(null);

        post.Title = request.Title;
        post.Slug = GenerateSlug(request.Title);
        post.Content = request.Content;
        post.Excerpt = request.Excerpt;
        post.CoverImageUrl = request.CoverImageUrl;
        post.PostType = request.PostType;

        return await Task.FromResult<PostDto?>(post);
    }

    public async Task<bool> PublishPostAsync(Guid postId)
    {
        await Task.Delay(300);
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if(post == null) return false;

        post.PublishedAt = DateTime.Now;
        return true;
    }

    public async Task<bool> UnPublishPostAsync (Guid postId)
    {
        await Task.Delay(300);
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if(post == null) return false;

        post.PublishedAt = null;
        return true;
    }

    private static string GenerateSlug(string title)
        => StringHelper.GenerateSlug(title);
}
