using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class CreateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string PostId { get; set; } = string.Empty;
}
