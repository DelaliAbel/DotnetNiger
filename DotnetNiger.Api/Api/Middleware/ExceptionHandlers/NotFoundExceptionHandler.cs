using Microsoft.AspNetCore.Diagnostics;

namespace DotnetNiger.Api.Middleware.ExceptionHandlers;

/// <summary>Traduit les <see cref="KeyNotFoundException"/> en 404.</summary>
public class NotFoundExceptionHandler : ExceptionHandlerBase
{
    public override async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not KeyNotFoundException)
            return false;

        await WriteErrorAsync(httpContext, StatusCodes.Status404NotFound, "Ressource introuvable", null, cancellationToken);
        return true;
    }
}
