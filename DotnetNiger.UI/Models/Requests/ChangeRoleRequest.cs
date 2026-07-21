namespace DotnetNiger.UI.Models.Requests;

public class ChangeRoleRequest
{
    public Guid UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;
    public bool IsRemove { get; set; }
}
