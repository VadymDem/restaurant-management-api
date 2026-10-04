using RRMS.Application.DTOs.Auth;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new LoginResponse(token);
    }

    public Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Register is planned for a later iteration.");
}