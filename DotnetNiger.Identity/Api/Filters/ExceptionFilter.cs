// Filtre API Identity: ExceptionFilter
using DotnetNiger.Identity.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetNiger.Identity.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
	// Conversion des exceptions metier en ProblemDetails.
	public void OnException(ExceptionContext context)
	{
		if (context.Exception is IdentityException identityException)
		{
			context.Result = new ObjectResult(new ProblemDetails
			{
				Status = identityException.StatusCode,
				Title = identityException.StatusCode == StatusCodes.Status401Unauthorized ? "Unauthorized" : "Identity error",
				Detail = identityException.Message,
				Instance = context.HttpContext.Request.Path
			})
			{
				StatusCode = identityException.StatusCode
			};

			if (context.Result is ObjectResult objectResult && objectResult.Value is ProblemDetails problem)
			{
				problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
			}

			context.ExceptionHandled = true;
			return;
		}

		context.Result = new ObjectResult(new ProblemDetails
		{
			Status = 500,
			Title = "Server error",
			Detail = "An unexpected error occurred.",
			Instance = context.HttpContext.Request.Path
		})
		{
			StatusCode = 500
		};

		if (context.Result is ObjectResult unhandledResult && unhandledResult.Value is ProblemDetails unhandledProblem)
		{
			unhandledProblem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
		}

		context.ExceptionHandled = true;
	}
}
