using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

[Route("api/certificates")]
public class CertificatesController : BaseController
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpPost]
    [Authorize(Policy = "community.certificates.submit")]
    public async Task<IActionResult> Submit([FromBody] CertificateSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CertificateUrl) || string.IsNullOrWhiteSpace(request.CertificateType))
            return BadRequest(new { Success = false, Message = "L'URL et le type du certificat sont requis." });

        var userId = GetUserId();
        var cert = await _certificateService.SubmitCertificateAsync(userId, request);
        return Ok(new { Success = true, Data = cert });
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        var cert = await _certificateService.GetUserCertificateAsync(userId);
        return Ok(new { Success = true, Data = cert });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cert = await _certificateService.GetCertificateAsync(id);
        if (cert == null)
            return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }

    [HttpGet]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var certs = await _certificateService.GetCertificatesAsync(status);
        return Ok(new { Success = true, Data = certs });
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var cert = await _certificateService.ApproveCertificateAsync(id);
        if (cert == null)
            return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Policy = "community.certificates.approve")]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });
        var cert = await _certificateService.RejectCertificateAsync(id, reason);
        if (cert == null)
            return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }
}
