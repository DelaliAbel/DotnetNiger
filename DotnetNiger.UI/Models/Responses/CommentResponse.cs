namespace DotnetNiger.UI.Models.Responses;

public class CommentResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReportedAt { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Status { get; set; } = "pending";
    public string? PostTitle { get; set; }
    public string? EventTitle { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();
}
