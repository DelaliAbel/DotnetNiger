using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Permet aux visiteurs d'envoyer des messages de contact à l'équipe.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactController(IContactService contactService) : ControllerBase
{
    /// <summary>Envoie un message de contact (nom, email, sujet, message).</summary>
    /// <param name="request">Informations du formulaire de contact.</param>
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
