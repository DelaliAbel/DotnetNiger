namespace DotnetNiger.Identity.Application.DTOs.Responses;

// Reponse pour une permission.
// Response for a permission.
public class PermissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
