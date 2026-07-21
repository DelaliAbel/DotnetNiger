using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class CreateTagRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
