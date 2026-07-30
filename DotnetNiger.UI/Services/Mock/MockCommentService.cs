using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockCommentService : ICommentService
{
    private const string CurrentUserName = "Vous";
    private const string CurrentUserAvatar = "/Images/default-avatar.jpg";
    private List<CommentResponse> _comments = new();
    private readonly IUserStateService _userStateService;

    public Task<Guid> GetCurrentUserIdAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CurrentUserId);

    private Guid CurrentUserId =>
        _userStateService.CurrentUser?.Id ?? Guid.Parse("11111111-1111-1111-1111-111111111111");

    public MockCommentService(IUserStateService userStateService)
    {
        _userStateService = userStateService;
        InitializeComments();
    }

    private void InitializeComments()
    {
        var eventId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var eventId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        
        // Post IDs for blog posts
        var postId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var postId2 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        
        // Reply IDs for nested comments
        var reply1Id = Guid.NewGuid();
        var reply2Id = Guid.NewGuid();

        _comments = new List<CommentResponse>
        {
            // Event comments
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId1,
                PostId = Guid.Empty,
                UserId = userId1,
                AuthorName = "Abdoulaye T.",
                AuthorAvatar = "/Images/user1.jpg",
                Content = "Est-ce qu'il y aura un atelier pratique sur .NET Aspire pendant la session cloud ? C'est le sujet qui m'intéresse le plus en ce moment.",
                CreatedAt = DateTime.Now.AddDays(-2),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "pending",
                EventTitle = "Conférence Cloud 2026",
                Replies = new List<CommentResponse>()
            },
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId1,
                PostId = Guid.Empty,
                UserId = userId2,
                AuthorName = "Mariam O.",
                AuthorAvatar = "/Images/user2.jpg",
                Content = "Hâte de participer à cette édition ! Les intervenants sont vraiment de grande qualité cette année. Le format hybride est une excellente idée.",
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "approved",
                EventTitle = "Conférence Cloud 2026",
                Replies = new List<CommentResponse>
                {
                    new CommentResponse
                    {
                        Id = reply1Id,
                        EventId = eventId1,
                        PostId = Guid.Empty,
                        UserId = userId3,
                        AuthorName = "Ahmed M.",
                        AuthorAvatar = "/Images/user3.jpg",
                        Content = "Totalement d'accord ! Je suis particulièrement intéressé par la session sur les performances.",
                        CreatedAt = DateTime.Now.AddDays(-2),
                        UpdatedAt = null,
                        ParentCommentId = Guid.Empty,
                        Status = "approved",
                        EventTitle = "Conférence Cloud 2026",
                        Replies = new List<CommentResponse>()
                    }
                }
            },
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId2,
                PostId = Guid.Empty,
                UserId = userId3,
                AuthorName = "Ahmed M.",
                AuthorAvatar = "/Images/user3.jpg",
                Content = "Y a-t-il des places disponibles pour les étudiants ? Une réduction est-elle prévue ?",
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "pending",
                EventTitle = "Atelier Blazor .NET 8",
                Replies = new List<CommentResponse>()
            },
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId1,
                PostId = Guid.Empty,
            UserId = CurrentUserId,
                AuthorName = CurrentUserName,
                AuthorAvatar = CurrentUserAvatar,
                Content = "Je confirme ma présence pour cet événement.",
                CreatedAt = DateTime.Now.AddHours(-8),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "approved",
                EventTitle = "Conférence Cloud 2026",
                Replies = new List<CommentResponse>()
            },
            
            // Blog post comments
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = Guid.Empty,
                PostId = postId1,
                UserId = userId1,
                AuthorName = "Abdoulaye T.",
                AuthorAvatar = "/Images/user1.jpg",
                Content = "Excellente explication sur C# 13 ! J'ai particulièrement apprécié la section sur les params collections. Cela va vraiment simplifier notre code.",
                CreatedAt = DateTime.Now.AddDays(-2),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "approved",
                PostTitle = "Les nouveautés de C# 13",
                Replies = new List<CommentResponse>
                {
                    new CommentResponse
                    {
                        Id = reply2Id,
                        EventId = Guid.Empty,
                        PostId = postId1,
                        UserId = userId2,
                        AuthorName = "Mariam O.",
                        AuthorAvatar = "/Images/user2.jpg",
                        Content = "Totalement d'accord ! C'est une amélioration majeure pour la flexibilité des APIs.",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        UpdatedAt = null,
                        ParentCommentId = Guid.Empty,
                        Status = "approved",
                        PostTitle = "Les nouveautés de C# 13",
                        Replies = new List<CommentResponse>()
                    }
                }
            },
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = Guid.Empty,
                PostId = postId1,
                UserId = userId2,
                AuthorName = "Mariam O.",
                AuthorAvatar = "/Images/user2.jpg",
                Content = "L'initiative pour la communauté .NET au Niger est inspirante ! Merci de promouvoir la technologie locale.",
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "rejected",
                PostTitle = "Les nouveautés de C# 13",
                Replies = new List<CommentResponse>()
            },
            new CommentResponse
            {
                Id = Guid.NewGuid(),
                EventId = Guid.Empty,
                PostId = postId2,
                UserId = userId3,
                AuthorName = "Ahmed M.",
                AuthorAvatar = "/Images/user3.jpg",
                Content = "Article très intéressant ! Quand prévoyez-vous le prochain atelier ?",
                CreatedAt = DateTime.Now.AddHours(-12),
                UpdatedAt = null,
                ParentCommentId = null,
                Status = "pending",
                PostTitle = "Guide complet ASP.NET Core 8",
                Replies = new List<CommentResponse>()
            }
        };
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

    public async Task<CommentResponse?> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(800);
        var comment = _comments.FirstOrDefault(c => c.Id == id);
        return comment;
    }

    public Task<CommentResponse?> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
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

    public Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request, CancellationToken cancellationToken = default)
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

    public Task<bool> DeleteCommentAsync(DeleteCommentRequest request, CancellationToken cancellationToken = default)
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

    public Task<CommentResponse?> ApproveCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = FindComment(_comments, id);
        if (comment == null)
            return Task.FromResult<CommentResponse?>(null);

        comment.Status = "approved";
        comment.UpdatedAt = DateTime.Now;
        return Task.FromResult<CommentResponse?>(comment);
    }

    public Task<CommentResponse?> RejectCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = FindComment(_comments, id);
        if (comment == null)
            return Task.FromResult<CommentResponse?>(null);

        comment.Status = "rejected";
        comment.UpdatedAt = DateTime.Now;
        return Task.FromResult<CommentResponse?>(comment);
    }

    private static CommentResponse? FindComment(List<CommentResponse> comments, Guid id)
    {
        foreach (var c in comments)
        {
            if (c.Id == id) return c;
            var found = FindComment(c.Replies, id);
            if (found != null) return found;
        }
        return null;
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
