namespace RRMS.Application.Exceptions;

/// <summary>
/// Base exception for business errors. The <see cref="StatusCode"/> is mapped
/// to an HTTP status code by the API exception-handling middleware.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}