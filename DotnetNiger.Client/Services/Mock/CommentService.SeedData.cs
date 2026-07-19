using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Mock;

public partial class CommentService
{
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
}
