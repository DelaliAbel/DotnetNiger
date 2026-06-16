using Asp.Versioning;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactController(IContactService contactService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { Success = false, Message = "Tous les champs sont requis." });
        }

        var result = await contactService.SendAsync(request);
        return Ok(new { Success = result, Message = result ? "Message envoyé avec succès." : "Erreur lors de l'envoi." });
    }
}
