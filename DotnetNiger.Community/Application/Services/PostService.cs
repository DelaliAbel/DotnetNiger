using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class PostService : IPostService
{
    private readonly IPostPersistence _postRepository;
    private readonly ISlugGenerator _slugGenerator;

    public PostService(IPostPersistence postRepository, ISlugGenerator slugGenerator)
    {
        _postRepository = postRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Post>> GetAllPublishedPostsAsync(int page = 1, int pageSize = 10)
    {
        return await _postRepository.GetPublishedPostsAsync(page, pageSize);
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        return await _postRepository.GetByIdAsync(id);
    }

    public async Task<Post?> GetPostBySlugAsync(string slug)
    {
        return await _postRepository.GetBySlugAsync(slug);
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post), "Post cannot be null");

        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        post.Slug = _slugGenerator.Generate(post.Title);
        return await _postRepository.AddAsync(post);
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post), "Post cannot be null");

        if (post.Id == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(post));

        post.UpdatedAt = DateTime.UtcNow;
        post.Slug = _slugGenerator.Generate(post.Title);
        return await _postRepository.UpdateAsync(post);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        return await _postRepository.DeleteAsync(id);
    }
}

