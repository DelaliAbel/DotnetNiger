using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record ConsentRequest(
    [Required] string ConsentType,
    [Required] string ConsentVersion,
    bool Granted = true);
