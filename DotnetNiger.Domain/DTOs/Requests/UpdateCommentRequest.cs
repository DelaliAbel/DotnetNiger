using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public class UpdateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
