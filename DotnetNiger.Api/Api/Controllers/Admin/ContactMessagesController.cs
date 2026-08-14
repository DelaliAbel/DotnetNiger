using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Admin;

/// <summary>Contrôleur de gestion des messages de contact (admin).</summary>
[ApiController]
[Route("api/admin/contact-messages")]
[Authorize(Policy = "admin.settings.manage")]
public class ContactMessagesController(IContactService contactService) : BaseController
{
    /// <summary>Récupère tous les messages de contact.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var messages = await contactService.GetAllAsync(ct);
        return Success(messages);
    }

    /// <summary>Marque un message comme lu.</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var result = await contactService.MarkAsReadAsync(id, ct);
        if (!result)
            return NotFound("Message non trouvé");
        return Success<object?>(null, "Message marqué comme lu");
    }
}
