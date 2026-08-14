using Microsoft.AspNetCore.Diagnostics;

namespace DotnetNiger.Api.Middleware.ExceptionHandlers;

/// <summary>Traduit les <see cref="InvalidOperationException"/> (validation métier) en 400.</summary>
public class InvalidOperationExceptionHandler : ExceptionHandlerBase
{
    public override async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not InvalidOperationException)
            return false;

        await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, exception.Message, null, cancellationToken);
        return true;
    }
}
