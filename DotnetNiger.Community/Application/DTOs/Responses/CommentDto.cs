namespace DotnetNiger.Community.Application.DTOs.Responses;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsApproved { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}
