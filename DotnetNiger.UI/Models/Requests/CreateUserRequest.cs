namespace DotnetNiger.UI.Models.Requests;

public class CreateUserRequest
{
      public string FullName { get; set; } = string.Empty;
      public string Email { get; set; } = string.Empty;
      public string Password { get; set; } = string.Empty;
      public bool IsTeamMember { get; set; }
      public string Position { get; set; } = string.Empty;

      public bool IsCollaborator { get; set; }

      public bool IsAdmin { get; set; }
      
      public bool HasApprovedCertificate { get; set; }
}