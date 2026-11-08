using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RRMS.Application.Options;
using RRMS.Application.Services;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

using MsOptions = Microsoft.Extensions.Options.Options;

namespace RRMS.Application.Tests;

public class JwtTokenServiceTests
{
    private const string SecretKey = "unit-test-secret-key-at-least-32-chars!";

    private static JwtOptions ValidOptions() => new()
    {
        Issuer = "rrms-test-issuer",
        Audience = "rrms-test-audience",
        SecretKey = SecretKey,
        ExpirationMinutes = 30
    };

    private static JwtTokenService CreateService(JwtOptions? options = null)
        => new(MsOptions.Create(options ?? ValidOptions()));

    private static User CreateUser(UserRole role = UserRole.Customer) => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        Role = role
    };

    private static JwtSecurityToken Read(string token)
        => new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static string? RoleClaim(JwtSecurityToken token)
        => token.Claims.FirstOrDefault(
            c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

    [Fact]
    public void GenerateToken_ContainsSubjectAndEmail()
    {
        var user = CreateUser();

        var token = Read(CreateService().GenerateToken(user));

        Assert.Equal(user.Id.ToString(), token.Subject);
        Assert.Equal(user.Email,
            token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Theory]
    [InlineData(UserRole.Customer)]
    [InlineData(UserRole.Administrator)]
    public void GenerateToken_ContainsUserRole(UserRole role)
    {
        var token = Read(CreateService().GenerateToken(CreateUser(role)));

        Assert.Equal(role.ToString(), RoleClaim(token));
    }

    [Fact]
    public void GenerateToken_UsesConfiguredIssuerAndAudience()
    {
        var token = Read(CreateService().GenerateToken(CreateUser()));

        Assert.Equal("rrms-test-issuer", token.Issuer);
        Assert.Contains("rrms-test-audience", token.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresAfterConfiguredMinutes()
    {
        var before = DateTime.UtcNow;

        var token = Read(CreateService().GenerateToken(CreateUser()));

        var after = DateTime.UtcNow;
        var tolerance = TimeSpan.FromSeconds(5);

        Assert.InRange(token.ValidTo,
            before.AddMinutes(30) - tolerance,
            after.AddMinutes(30) + tolerance);
        Assert.InRange(token.ValidFrom,
            before - tolerance,
            after + tolerance);
    }

    [Fact]
    public void GenerateToken_IsSignedWithConfiguredKey()
    {
        var jwt = CreateService().GenerateToken(CreateUser());

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            jwt,
            BuildValidationParameters(SecretKey),
            out var validated);

        Assert.NotNull(principal);
        Assert.IsType<JwtSecurityToken>(validated);
    }

    [Fact]
    public void GenerateToken_FailsValidationWithDifferentKey()
    {
        var jwt = CreateService().GenerateToken(CreateUser());

        Assert.ThrowsAny<SecurityTokenException>(
            () => new JwtSecurityTokenHandler().ValidateToken(
                jwt,
                BuildValidationParameters("another-secret-key-at-least-32-chars!!"),
                out _));
    }

    [Fact]
    public void GenerateToken_FailsValidationWithDifferentAudience()
    {
        var jwt = CreateService().GenerateToken(CreateUser());
        var parameters = BuildValidationParameters(SecretKey);
        parameters.ValidAudience = "someone-else";

        Assert.ThrowsAny<SecurityTokenException>(
            () => new JwtSecurityTokenHandler().ValidateToken(
                jwt, parameters, out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-short-key")]
    public void GenerateToken_InvalidSecretKey_Throws(string? secret)
    {
        var options = ValidOptions();
        options.SecretKey = secret!;

        Assert.Throws<InvalidOperationException>(
            () => CreateService(options).GenerateToken(CreateUser()));
    }

    [Fact]
    public void GenerateToken_SecretKeyOfExactlyMinLength_IsAccepted()
    {
        var options = ValidOptions();
        options.SecretKey = new string('k', 32);

        var jwt = CreateService(options).GenerateToken(CreateUser());

        Assert.False(string.IsNullOrWhiteSpace(jwt));
    }

    private static TokenValidationParameters BuildValidationParameters(string key)
        => new()
        {
            ValidateIssuer = true,
            ValidIssuer = "rrms-test-issuer",
            ValidateAudience = true,
            ValidAudience = "rrms-test-audience",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
}