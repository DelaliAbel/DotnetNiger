using System.Net.Http.Json;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiRegistrationService : ApiServiceBase, IRegistrationService
{
    public ApiRegistrationService(HttpClient http, ILogger<ApiRegistrationService> logger) : base(http, logger) { }

    public Task<ApiSuccessResponse<Guid>> SubmitStep1Async(RegisterRequest request)
    {
        throw new NotSupportedException("L'étape 1 est gérée par DotnetNiger Identity via redirection externe.");
    }

    public async Task<ApiSuccessResponse<CertificateStatusDto>> SubmitStep2Async(CertificateSubmissionDto request)
    {
        var url = ApiEndpoints.Certificates;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                var errorBody = await response.Content.ReadAsStringAsync();
                return new ApiSuccessResponse<CertificateStatusDto>
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(errorBody) ? "Erreur lors de la soumission du certificat." : errorBody
                };
            }

            var result = await ApiResponseReader.ReadAsync<CertificateStatusDto>(response);
            return result is not null
                ? new ApiSuccessResponse<CertificateStatusDto> { Success = true, Data = result }
                : new ApiSuccessResponse<CertificateStatusDto> { Success = true };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return new ApiSuccessResponse<CertificateStatusDto>
            {
                Success = false,
                Message = $"Erreur réseau lors de la soumission du certificat: {ex.Message}"
            };
        }
    }
}
