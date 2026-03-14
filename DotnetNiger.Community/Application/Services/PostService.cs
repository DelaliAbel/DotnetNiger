using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ISlugGenerator _slugGenerator;

    public PostService(IPostRepository postRepository, ISlugGenerator slugGenerator)
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
        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        post.Slug = _slugGenerator.Generate(post.Title);
        return await _postRepository.AddAsync(post);
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        post.Slug = _slugGenerator.Generate(post.Title);
        return await _postRepository.UpdateAsync(post);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        return await _postRepository.DeleteAsync(id);
    }
}

