using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record ConfirmChangeEmailRequest(
    [Required][EmailAddress] string NewEmail,
    [Required] string Code);
