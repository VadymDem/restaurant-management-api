
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Infrastructure.Data;

public static class DbSeeder
{
    /// <summary>
    /// Creates a development-only Administrator account when AdminSeed
    /// credentials are configured and the account does not exist.
    /// </summary>
    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        // Never seed a test administrator outside Development.
        if (!environment.IsDevelopment())
        {
            return;
        }

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        email = email.Trim();

        var userExists = await dbContext.Users.AnyAsync(
            u => u.Email == email,
            cancellationToken);

        if (userExists)
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = configuration["AdminSeed:Name"]
                ?? "Administrator",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Administrator,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
