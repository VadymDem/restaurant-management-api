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

    public AuthService(
     IUnitOfWork unitOfWork,
     IJwtTokenService jwtTokenService,
     IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(
            request.Email.Trim(),
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
        catch
        {
            // Registration should still succeed even if the welcome email fails.
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(token);
    }
}