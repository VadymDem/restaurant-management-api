namespace RRMS.Api.Middleware;

/// <summary>
/// Centralizes exception handling so the API always returns a consistent JSON error body.
/// </summary>
public class ExceptionHandlingMiddleware
{
    // TODO: constructor should take RequestDelegate next, ILogger<ExceptionHandlingMiddleware> and IHostEnvironment.
    // TODO: InvokeAsync(HttpContext context):
    //   - try { await _next(context); }
    //   - catch known domain errors -> 400/403/404 ProblemDetails;
    //   - catch any other exception -> log it and return 500 ProblemDetails (see RRMS.Api.DTOs.ErrorResponse).
}