// DTO request Identity: AddPermissionRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

// Requete de creation d'une permission.
public class AddPermissionRequest
{
	[Required]
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;
}
