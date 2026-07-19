using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Mock;

public partial class CommentService : ICommentService
{
    private const string CurrentUserName = "Vous";
    private const string CurrentUserAvatar = "/Images/default-avatar.jpg";
    private List<CommentResponse> _comments = new();
    private readonly IUserStateService _userStateService;

    public Task<Guid> GetCurrentUserIdAsync() =>
        Task.FromResult(CurrentUserId);

    private Guid CurrentUserId =>
        _userStateService.CurrentUser?.Id ?? Guid.Parse("11111111-1111-1111-1111-111111111111");

    public CommentService(IUserStateService userStateService)
    {
        _userStateService = userStateService;
        InitializeComments();
    }

    public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId)
    {
        await Task.Delay(800);
        var comments = _comments
            .Where(c => c.PostId == postId && c.ParentCommentId is null)
            .Select(CloneCommentTree)
            .ToList();
        return comments;
    }

    public async Task<List<CommentResponse>> GetCommentsByEventIdAsync(Guid eventId)
    {
        await Task.Delay(800);
        var comments = _comments
            .Where(c => c.EventId == eventId && c.ParentCommentId is null)
            .Select(CloneCommentTree)
            .ToList();
        return comments;
    }

    public async Task<CommentResponse?> GetCommentByIdAsync(Guid id)
    {
        await Task.Delay(800);
        var comment = _comments.FirstOrDefault(c => c.Id == id);
        return comment;
    }

    public Task<CommentResponse?> CreateCommentAsync(CreateCommentRequest request)
    {
        var newComment = new CommentResponse
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId ?? Guid.Empty,
            PostId = request.PostId ?? Guid.Empty,
            UserId = CurrentUserId,
            AuthorName = CurrentUserName,
            AuthorAvatar = CurrentUserAvatar,
            Content = request.Content,
            CreatedAt = DateTime.Now,
            UpdatedAt = null,
            ParentCommentId = request.ParentCommentId,
            Replies = new List<CommentResponse>()
        };

        _comments.Add(newComment);
        
        // If this is a reply, add it to parent's replies
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = _comments.FirstOrDefault(c => c.Id == request.ParentCommentId.Value);
            if (parentComment != null)
            {
                parentComment.Replies.Add(newComment);
            }
        }
        
        return Task.FromResult<CommentResponse?>(newComment);
    }

    public Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == request.Id);
        if (comment == null)
            return Task.FromResult<CommentResponse?>(null);

        if (comment.UserId != CurrentUserId)
            return Task.FromResult<CommentResponse?>(null);

        comment.Content = request.Content ?? comment.Content;
        comment.UpdatedAt = DateTime.Now;
        return Task.FromResult<CommentResponse?>(comment);
    }

    public Task<bool> DeleteCommentAsync(DeleteCommentRequest request)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == request.Id);
        if (comment == null)
            return Task.FromResult(false);

        if (comment.UserId != CurrentUserId)
            return Task.FromResult(false);

        if (request.DeleteAllReplies)
        {
            _comments.RemoveAll(c => c.ParentCommentId == request.Id || c.Id == request.Id);
        }
        else
        {
            _comments.Remove(comment);
        }

        return Task.FromResult(true);
    }

    private static CommentResponse CloneCommentTree(CommentResponse comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            EventId = comment.EventId,
            PostId = comment.PostId,
            UserId = comment.UserId,
            AuthorName = comment.AuthorName,
            AuthorAvatar = comment.AuthorAvatar,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies.Select(CloneCommentTree).ToList()
        };
    }

    public Task<List<CommentResponse>> GetAllCommentsAsync()
    {
        var flat = new List<CommentResponse>();
        foreach (var c in _comments)
        {
            flat.Add(c);
            FlattenReplies(c, flat);
        }
        return Task.FromResult(flat);
    }

    private static void FlattenReplies(CommentResponse comment, List<CommentResponse> flat)
    {
        foreach (var reply in comment.Replies)
        {
            flat.Add(reply);
            FlattenReplies(reply, flat);
        }
    }
}
