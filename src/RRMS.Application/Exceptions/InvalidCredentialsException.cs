namespace RRMS.Application.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base(401, "Invalid email or password.")
    {
    }
}