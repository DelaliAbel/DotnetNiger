using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class ApproveResourceRequest
{
    [Required]
    public Guid ResourceId { get; set; }

    public bool IsApproved { get; set; } = true;
}
