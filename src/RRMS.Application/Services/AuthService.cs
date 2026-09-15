using RRMS.Application.DTOs.Auth;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Application.Services;

public class AuthService : IAuthService
{
    // TODO: inject IUserRepository, IUnitOfWork and IJwtTokenService via constructor.
    // TODO: decide on a password hashing strategy (e.g. BCrypt.Net-Next or ASP.NET Core Identity).

    public Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: check that email is not registered, hash the password, create a User with Role = Customer, save and return a JWT token.");

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: find user by email, verify the password hash and return a JWT token with the user's role claim.");
}