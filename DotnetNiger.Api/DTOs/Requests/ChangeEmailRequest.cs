using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record ChangeEmailRequest(
    [Required][EmailAddress] string NewEmail);
