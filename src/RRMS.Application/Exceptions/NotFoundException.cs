namespace RRMS.Application.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(404, message)
    {
    }
}