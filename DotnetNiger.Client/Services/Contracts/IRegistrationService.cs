using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IRegistrationService
{
    Task<ApiSuccessResponse<Guid>> SubmitStep1Async(RegisterRequest request);
    Task<ApiSuccessResponse<CertificateStatusDto>> SubmitStep2Async(CertificateSubmissionDto request);
}
