using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class ApproveResourceRequest
{
    [Required]
    public Guid ResourceId { get; set; }

    public bool IsApproved { get; set; } = false;
}
