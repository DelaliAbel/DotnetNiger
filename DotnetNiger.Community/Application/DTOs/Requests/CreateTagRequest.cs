using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class CreateTagRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
