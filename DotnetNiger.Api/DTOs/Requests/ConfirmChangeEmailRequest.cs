using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record ConfirmChangeEmailRequest(
    [Required][EmailAddress] string NewEmail,
    [Required] string Code);
