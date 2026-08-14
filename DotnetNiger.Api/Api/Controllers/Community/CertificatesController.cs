using System.Threading;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des certificats des membres.</summary>
[ApiController]
[Route("api/certificates")]
public class CertificatesController : BaseController
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    /// <summary>Soumet un nouveau certificat pour validation.</summary>
    [HttpPost]
    [Authorize(Policy = "community.certificates.submit")]
    public async Task<IActionResult> Submit([FromBody] CertificateSubmissionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CertificateUrl) || string.IsNullOrWhiteSpace(request.CertificateType))
            return BadRequest("L'URL et le type du certificat sont requis.");

        var userId = GetUserId();
        var cert = await _certificateService.SubmitCertificateAsync(userId, request, ct);
        return Success(cert);
    }

    /// <summary>Récupère le certificat de l'utilisateur connecté.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var cert = await _certificateService.GetUserCertificateAsync(userId, ct);
        return Success(cert);
    }

    /// <summary>Récupère un certificat par son identifiant (admin).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var cert = await _certificateService.GetCertificateAsync(id, ct);
        if (cert == null)
            return NotFound(Messages.Certificate.NotFound);
        return Success(cert);
    }

    /// <summary>Récupère tous les certificats avec filtre par statut (admin).</summary>
    [HttpGet]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct = default)
    {
        var certs = await _certificateService.GetCertificatesAsync(status, ct);
        return Success(certs);
    }

    /// <summary>Approuve un certificat.</summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct = default)
    {
        var cert = await _certificateService.ApproveCertificateAsync(id, ct);
        if (cert == null)
            return NotFound(Messages.Certificate.NotFound);
        return Success(cert);
    }

    /// <summary>Rejette un certificat avec une raison.</summary>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(Messages.Certificate.RejectReasonRequired);
        var cert = await _certificateService.RejectCertificateAsync(id, reason, ct);
        if (cert == null)
            return NotFound(Messages.Certificate.NotFound);
        return Success(cert);
    }
}
