using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record ChangeEmailRequest(
    [Required][EmailAddress] string NewEmail);
