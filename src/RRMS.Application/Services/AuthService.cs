using Microsoft.Extensions.Logging;
using RRMS.Application.DTOs.Auth;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
     IUnitOfWork unitOfWork,
     IJwtTokenService jwtTokenService,
     IEmailService emailService,
     ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _unitOfWork.Users.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        if (!BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(token);
    }

    public async Task<LoginResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _unitOfWork.Users.ExistsByEmailAsync(
            email,
            cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                "User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        try
        {
            await _emailService.SendWelcomeEmailAsync(
                user.Email,
                user.Name,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Registration should still succeed even if the welcome email fails,
            // but the failure must stay visible in the logs.
            _logger.LogWarning(
                ex,
                "Failed to send welcome email to {Email}.",
                user.Email);
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(token);
    }
}