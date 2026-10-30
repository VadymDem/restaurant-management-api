namespace RRMS.Application.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base(401, message)
    {
    }
}
