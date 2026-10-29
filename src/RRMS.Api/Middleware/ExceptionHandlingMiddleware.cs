
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RRMS.Application.Exceptions;

namespace RRMS.Api.Middleware;

/// <summary>
/// Centralizes exception handling and returns consistent JSON errors.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
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
            _logger.LogWarning(
                ex,
                "Domain error {StatusCode}: {Message}",
                ex.StatusCode,
                ex.Message);

            await WriteProblemAsync(
                context,
                ex.StatusCode,
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request.");

            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ux_reservations_table_slot"
            })
        {
            _logger.LogWarning(
                ex,
                "Reservation slot conflict.");

            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "The selected table is already booked for this date and time.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception on {Path}.",
                context.Request.Path);

            var message = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred while processing the request.";

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                message);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "about:blank",
            title = ReasonPhrases.GetReasonPhrase(statusCode),
            status = statusCode,
            detail = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}