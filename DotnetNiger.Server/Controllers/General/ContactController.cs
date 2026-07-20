using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Server.Controllers.General;

[ApiController]
[Route("api/[controller]")]
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
            return BadRequest(new { Success = false, Message = Messages.Contact.AllFieldsRequired });
        }

        var result = await contactService.SendAsync(request);
        return Ok(new { Success = result, Message = result ? Messages.Contact.Sent : Messages.Contact.Error });
    }
}
