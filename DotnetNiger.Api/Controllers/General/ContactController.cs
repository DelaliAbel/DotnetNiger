using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de formulaire de contact.</summary>
[ApiController]
[Route("api/contact")]
public class ContactController(IContactService contactService) : ControllerBase
{
    /// <summary>Envoie un message de contact.</summary>
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { Success = false, Message = Messages.Contact.AllFieldsRequired });
        }

        var result = await contactService.SendAsync(request);
        return Ok(new { Success = result, Message = result ? Messages.Contact.Sent : Messages.Contact.Error });
    }
}
