using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class UpdateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
