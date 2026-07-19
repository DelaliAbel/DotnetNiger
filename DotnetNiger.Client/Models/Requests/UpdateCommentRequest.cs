using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Client.Models.Requests;

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    [Required]
    public Guid Id { get; set; }
}
