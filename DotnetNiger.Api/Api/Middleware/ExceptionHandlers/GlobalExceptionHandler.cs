using Microsoft.AspNetCore.Diagnostics;

namespace DotnetNiger.Api.Middleware.ExceptionHandlers;

/// <summary>Handler de dernier recours : toute exception non gérée devient un 500.</summary>
public class GlobalExceptionHandler : ExceptionHandlerBase
{
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public override async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var detail = _environment.IsDevelopment() ? exception.ToString() : null;
        await WriteErrorAsync(httpContext, StatusCodes.Status500InternalServerError, "Erreur interne du serveur", detail, cancellationToken);
        return true;
    }
}
