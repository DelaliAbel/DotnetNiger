using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid ParseGuidOrThrow(string value, string parameterName, string invalidMessage)
    {
        if (!Guid.TryParse(value, out var parsed))
        {
            throw new ArgumentException(invalidMessage, parameterName);
        }

        return parsed;
    }

    protected IActionResult BadRequestProblem(string detail, string title = "Requete invalide")
    {
        return BadRequest(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        });
    }

    protected IActionResult NotFoundProblem(string detail, string title = "Ressource introuvable")
    {
        return NotFound(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status404NotFound,
            Instance = HttpContext.Request.Path
        });
    }

    protected IActionResult Success<T>(T data, string? message = null, object? meta = null)
    {
        return Ok(new ApiSuccessResponse<T>
        {
            Data = data,
            Message = message,
            Meta = meta
        });
    }

    protected IActionResult SuccessMessage(string message, object? meta = null)
    {
        return Ok(new ApiSuccessResponse<object>
        {
            Data = null,
            Message = message,
            Meta = meta
        });
    }

    protected IActionResult CreatedSuccess<T>(string actionName, object? routeValues, T data, string? message = null, object? meta = null)
    {
        return CreatedAtAction(actionName, routeValues, new ApiSuccessResponse<T>
        {
            Data = data,
            Message = message,
            Meta = meta
        });
    }
}
