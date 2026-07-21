using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class UpdateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
