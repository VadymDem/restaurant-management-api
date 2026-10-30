using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private const string Endpoint = "https://api.resend.com/emails";

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

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Resend:ApiKey is not configured, so the welcome email cannot be sent.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new
            {
                from = "Restaurant Reservation <onboarding@resend.dev>",
                to = new[] { email },
                subject = "Welcome to Restaurant Reservation!",
                html = BuildWelcomeHtml(name, email)
            })
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(
            request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"Resend returned {(int)response.StatusCode} " +
                $"({response.StatusCode}): {body}");
        }
    }

    private static string BuildWelcomeHtml(string name, string email)
    {
        var safeName = HtmlEncoder.Default.Encode(name);
        var safeEmail = HtmlEncoder.Default.Encode(email);

        return $"""
        <h2>Welcome, {safeName}!</h2>

        <p>Your account at <strong>Restaurant Reservation</strong>
        has been successfully created.</p>

        <p>You can now sign in, browse the restaurant menu,
        and make reservations.</p>

        <p>Your account email: <strong>{safeEmail}</strong></p>

        <p>We look forward to seeing you!</p>

        <p>
            Best regards,<br>
            <strong>Restaurant Reservation Team</strong>
        </p>
        """;
    }
}
