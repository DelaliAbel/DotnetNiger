using System.Security.Claims;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
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

    protected IActionResult NotFoundProblem(string detail, string title = "Resource not found")
    {
        return NotFound(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status404NotFound,
            Instance = HttpContext.Request.Path
        });
    }

    protected IActionResult BadRequestProblem(string detail, string title = "Invalid request")
    {
        return BadRequest(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        });
    }

    protected Guid RequireAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new IdentityException("Authentication required. Please login.", StatusCodes.Status401Unauthorized);
        }

        return userId;
    }
}
