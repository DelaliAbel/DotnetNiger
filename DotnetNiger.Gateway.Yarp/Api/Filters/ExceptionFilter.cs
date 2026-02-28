using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetNiger.Gateway.Api.Filters;

/// <summary>
/// Filtre les exceptions non traitées
/// </summary>
public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Exception non gérée dans le controller");

        context.Result = new ObjectResult(new
        {
            error = "Une erreur s'est produite",
            message = context.Exception.Message
        })
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}
