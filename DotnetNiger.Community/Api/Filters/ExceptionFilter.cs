using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DotnetNiger.Community.Application.Exceptions;
using System.Net;

namespace DotnetNiger.Community.Api.Filters;

/// <summary>
/// Global exception filter to convert unhandled exceptions into ProblemDetails responses
/// Ensures consistency with HTTP status codes and error formatting
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
        var exception = context.Exception;
        var problemDetails = new ProblemDetails
        {
            Instance = context.HttpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        // Map exception types to HTTP responses
        switch (exception)
        {
            // 400 Bad Request
            case BadRequestException badEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = badEx.Message;
                break;

            // 404 Not Found
            case NotFoundException notFoundEx:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Not Found";
                problemDetails.Detail = notFoundEx.Message;
                break;

            // 401 Unauthorized
            case Application.Exceptions.UnauthorizedAccessException unauthorizedEx:
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = unauthorizedEx.Message ?? "You are not authorized to access this resource";
                break;

            // 403 Forbidden
            case ForbiddenException forbiddenEx:
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Forbidden";
                problemDetails.Detail = forbiddenEx.Message;
                break;

            // 409 Conflict
            case ConflictException conflictEx:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Conflict";
                problemDetails.Detail = conflictEx.Message;
                break;

            // 422 Unprocessable Entity
            case ValidationException validEx:
                problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "The request contains invalid data";
                // Include validation errors if available
                if (validEx.Errors != null && validEx.Errors.Any())
                {
                    problemDetails.Extensions["errors"] = validEx.Errors;
                }
                break;

            // 500 Internal Server Error (unhandled)
            default:
                _logger.LogError(exception, "Unhandled exception occurred");
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred. Please contact support if the problem persists.";

                // Only include detailed error info in development
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) &&
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                    problemDetails.Extensions["message"] = exception.Message;
                    if (exception.InnerException != null)
                    {
                        problemDetails.Extensions["innerException"] = exception.InnerException.Message;
                    }
                }
                break;
        }

        // Log the error
        _logger.LogError(exception, "Exception handled: {Title}", problemDetails.Title);

        // Return the formatted response
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };

        context.ExceptionHandled = true;
    }
}

// Custom exception classes for Community domain
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? Enumerable.Empty<string>();
    }
}
