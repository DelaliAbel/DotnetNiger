using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs;

public class CreateTagRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
