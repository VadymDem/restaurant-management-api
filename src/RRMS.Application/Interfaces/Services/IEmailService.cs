namespace RRMS.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default);
}