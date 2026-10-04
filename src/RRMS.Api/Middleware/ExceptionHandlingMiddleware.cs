using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using RRMS.Application.Exceptions;

namespace RRMS.Api.Middleware;

/// <summary>
/// Centralizes exception handling so the API always returns a consistent JSON error body.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error {StatusCode}: {Message}", ex.StatusCode, ex.Message);
            await WriteProblemAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}.", context.Request.Path);

            var message = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred while processing the request.";

            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, message);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var title = ReasonPhrases.GetReasonPhrase(statusCode);
        var problem = new
        {
            type = "about:blank",
            title,
            status = statusCode,
            detail = message,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}