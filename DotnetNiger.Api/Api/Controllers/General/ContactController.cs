using System.Threading;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de formulaire de contact.</summary>
[ApiController]
[Route("api/contact")]
[EnableRateLimiting("default")]
public class ContactController(IContactService contactService) : BaseController
{
    /// <summary>Envoie un message de contact.</summary>
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(Messages.Contact.AllFieldsRequired);
        }

        var result = await contactService.SendAsync(request, ct);
        return result ? Success<object?>(null, Messages.Contact.Sent) : Failure(Messages.Contact.Error);
    }
}
