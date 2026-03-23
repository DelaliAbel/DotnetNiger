using System.Diagnostics;

namespace DotnetNiger.Community.Api.Middleware;

/// <summary>
/// Middleware pour enregistrer les requêtes HTTP entrantes et les réponses sortantes
/// Enregistre le temps d'exécution et les informations de la requête/réponse
/// </summary>
public class RequestLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<RequestLoggingMiddleware> _logger;

	public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = Stopwatch.StartNew();

		// Log incoming request
		_logger.LogInformation(
			"HTTP Request: {Method} {Path} from {IP}",
			context.Request.Method,
			context.Request.Path,
			context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
		);

		// Copy the response stream so we can read it
		var originalBodyStream = context.Response.Body;
		using var responseBody = new MemoryStream();
		context.Response.Body = responseBody;

		try
		{
			await _next(context);
		}
		finally
		{
			stopwatch.Stop();

			// Log response
			_logger.LogInformation(
				"HTTP Response: {Method} {Path} - Status: {StatusCode} - Duration: {DurationMs}ms",
				context.Request.Method,
				context.Request.Path,
				context.Response.StatusCode,
				stopwatch.ElapsedMilliseconds
			);

			// Copy the response back to the original stream
			await responseBody.CopyToAsync(originalBodyStream);
		}
	}
}
