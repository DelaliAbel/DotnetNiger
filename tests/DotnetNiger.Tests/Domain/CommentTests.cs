using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie les règles de publication et de modération d'un commentaire.
/// </summary>
public sealed class CommentTests
{
    private static readonly Guid Auteur = Guid.NewGuid();

    [Fact]
    public void ForArticle_AttachesToArticleAndPublishes()
    {
        var articleId = Guid.NewGuid();

        var commentaire = Comment.ForArticle(articleId, Auteur, "Bonjour");

        Assert.Equal(articleId, commentaire.ArticleId);
        Assert.Null(commentaire.CommunityEventId);
        Assert.Equal(CommentStatus.Published, commentaire.Status);
        Assert.Equal(Auteur, commentaire.AuthorId);
        Assert.Equal("Bonjour", commentaire.Body);
    }

    [Fact]
    public void ForCommunityEvent_AttachesToEventOnly()
    {
        var evenementId = Guid.NewGuid();

        var commentaire = Comment.ForCommunityEvent(evenementId, Auteur, "Bonjour");

        Assert.Equal(evenementId, commentaire.CommunityEventId);
        Assert.Null(commentaire.ArticleId);
    }

    [Fact]
    public void ForArticle_Throws_WhenTargetMissing()
    {
        var erreur = Assert.Throws<DomainException>(
            () => Comment.ForArticle(Guid.Empty, Auteur, "Bonjour"));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void ForArticle_Throws_WhenAuthorMissing()
    {
        var erreur = Assert.Throws<DomainException>(
            () => Comment.ForArticle(Guid.NewGuid(), Guid.Empty, "Bonjour"));

        Assert.Equal("field_required", erreur.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Throws_WhenBodyEmpty(string? corps)
    {
        var erreur = Assert.Throws<DomainException>(
            () => Comment.ForArticle(Guid.NewGuid(), Auteur, corps!));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Create_Throws_WhenBodyTooLong()
    {
        var erreur = Assert.Throws<DomainException>(
            () => Comment.ForArticle(Guid.NewGuid(), Auteur, new string('a', 4_001)));

        Assert.Equal("field_too_long", erreur.Code);
    }

    [Fact]
    public void Create_TrimsBody()
    {
        var commentaire = Comment.ForArticle(Guid.NewGuid(), Auteur, "  Bonjour  ");

        Assert.Equal("Bonjour", commentaire.Body);
    }

    [Fact]
    public void ReplyTo_InheritsArticleTargetAndLinksParent()
    {
        var parent = Comment.ForArticle(Guid.NewGuid(), Auteur, "Question");

        var reponse = Comment.ReplyTo(parent, Guid.NewGuid(), "Réponse");

        Assert.Equal(parent.ArticleId, reponse.ArticleId);
        Assert.Equal(parent.Id, reponse.ParentCommentId);
    }

    [Fact]
    public void ReplyTo_InheritsEventTarget()
    {
        var parent = Comment.ForCommunityEvent(Guid.NewGuid(), Auteur, "Question");

        var reponse = Comment.ReplyTo(parent, Guid.NewGuid(), "Réponse");

        Assert.Equal(parent.CommunityEventId, reponse.CommunityEventId);
        Assert.Null(reponse.ArticleId);
    }

    [Fact]
    public void ReplyTo_Throws_WhenParentHasNoTarget()
    {
        // Un parent reconstruit par EF sans cible est le seul cas atteignable en pratique.
        var orphelin = Comment.ForCommunityEvent(Guid.NewGuid(), Auteur, "Autre");
        typeof(Comment).GetProperty(nameof(Comment.CommunityEventId))!
            .SetValue(orphelin, null);

        var erreur = Assert.Throws<DomainException>(
            () => Comment.ReplyTo(orphelin, Auteur, "Réponse"));

        Assert.Equal("comment_invalid_parent", erreur.Code);
    }

    [Fact]
    public void ReplyTo_Throws_WhenParentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Comment.ReplyTo(null!, Auteur, "Réponse"));
    }

    [Fact]
    public void ModerationTransitions_ChangeStatusOnly()
    {
        var commentaire = Comment.ForArticle(Guid.NewGuid(), Auteur, "Bonjour");

        commentaire.Hide();
        Assert.Equal(CommentStatus.Hidden, commentaire.Status);

        commentaire.MarkApproved();
        Assert.Equal(CommentStatus.Published, commentaire.Status);

        commentaire.MarkPendingReview();
        Assert.Equal(CommentStatus.PendingReview, commentaire.Status);

        commentaire.MarkDeleted();
        Assert.Equal(CommentStatus.Deleted, commentaire.Status);
    }
}
