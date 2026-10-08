using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ResendEmailService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendWelcomeEmailAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Resend:ApiKey"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var request = new
        {
            from = "Restaurant Reservation <onboarding@resend.dev>",
            to = new[] { email },
            subject = "Welcome to Restaurant Reservation!",
            html = $"""
        <h2>Welcome, {name}!</h2>

        <p>Your account at <strong>Restaurant Reservation</strong>
        has been successfully created.</p>

        <p>You can now sign in, browse the restaurant menu,
        and make reservations.</p>

        <p>Your account email: <strong>{email}</strong></p>

        <p>We look forward to seeing you!</p>

        <p>
            Best regards,<br>
            <strong>Restaurant Reservation Team</strong>
        </p>
        """
        };

        await _httpClient.PostAsJsonAsync(
            "https://api.resend.com/emails",
            request,
            cancellationToken);
    }
}