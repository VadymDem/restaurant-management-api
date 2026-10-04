namespace RRMS.Application.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(409, message)
    {
    }
}