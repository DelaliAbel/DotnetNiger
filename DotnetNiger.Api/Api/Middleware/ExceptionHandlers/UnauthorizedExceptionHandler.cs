using Microsoft.AspNetCore.Diagnostics;

namespace DotnetNiger.Api.Middleware.ExceptionHandlers;

/// <summary>Traduit les <see cref="UnauthorizedAccessException"/> en 403.</summary>
public class UnauthorizedExceptionHandler : ExceptionHandlerBase
{
    public override async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
            return false;

        await WriteErrorAsync(httpContext, StatusCodes.Status403Forbidden, "Accès refusé", null, cancellationToken);
        return true;
    }
}
