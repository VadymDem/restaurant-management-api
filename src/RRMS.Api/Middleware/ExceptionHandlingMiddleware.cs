
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
            when (ex.InnerException is PostgresException postgresException)
        {
            var (statusCode, message) = MapDatabaseError(postgresException);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    ex,
                    "Database error {SqlState} ({Constraint}) on {Path}.",
                    postgresException.SqlState,
                    postgresException.ConstraintName,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    ex,
                    "Constraint {Constraint} rejected the request on {Path}.",
                    postgresException.ConstraintName,
                    context.Request.Path);
            }

            await WriteProblemAsync(context, statusCode, message);
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

    private static (int StatusCode, string Message) MapDatabaseError(
        PostgresException exception)
        => exception.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => (
                StatusCodes.Status409Conflict,
                exception.ConstraintName switch
                {
                    "ux_reservations_table_slot" =>
                        "The selected table is already booked for this date and time.",
                    "ix_users_email" =>
                        "A user with this email already exists.",
                    "Tables_table_number_key" =>
                        "A table with this number already exists.",
                    _ =>
                        "The request conflicts with an existing record."
                }),
            PostgresErrorCodes.ForeignKeyViolation => (
                StatusCodes.Status409Conflict,
                "The request references a record that does not exist."),
            PostgresErrorCodes.CheckViolation => (
                StatusCodes.Status400BadRequest,
                "The request violates a data constraint."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected database error occurred while processing the request.")
        };

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