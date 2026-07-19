using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Mock;

public partial class MockAuthService
{
    #region Gestion de compte

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await Task.Delay(500);
        
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return true;
        }

        return true;
    }

    public async Task<ApiSuccessResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        await Task.Delay(500);
        
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return new ApiSuccessResponse<object>
            {
                Success = false,
                Message = "Email invalide",
                Data = false
            };
        }

        // Simuler la vérification du token
        if (request.Token != "valid-reset-token")
        {
            return new ApiSuccessResponse<object>
            {
                Success = false,
                Message = "Token invalide ou expiré",
                Data = false
            };
        }

        return new ApiSuccessResponse<object>
        {
            Success = true,
            Message = "Votre mot de passe a été réinitialisé avec succès.",
            Data = true
        };
    }

    public async Task<bool> RequestEmailVerificationAsync(RequestEmailVerificationRequest request)
    {
        await Task.Delay(500);
        
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        await Task.Delay(500);
        
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return false;
        }

        // Simuler la vérification du token
        if (request.Code != "valid-verification-code")
        {
            return false;
        }

        return true;
    }

    #endregion
}
