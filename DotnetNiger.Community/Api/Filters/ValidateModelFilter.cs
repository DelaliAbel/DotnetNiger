using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetNiger.Community.Api.Filters;

/// <summary>
/// Action filter pour valider automatiquement le ModelState
/// Retourne une réponse 400 BadRequest si la validation échoue
/// </summary>
public class ValidateModelFilter : IActionFilter
{
    private readonly ILogger<ValidateModelFilter> _logger;

    public ValidateModelFilter(ILogger<ValidateModelFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed for action {ActionName}",
                context.ActionDescriptor.DisplayName);

            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .ToDictionary(
                    ms => ms.Key,
                    ms => ms.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The request body contains invalid data. Please check the errors field.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["errors"] = errors;

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to do after action execution
    }
}
