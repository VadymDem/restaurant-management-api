using RRMS.Application.DTOs.Auth;

namespace RRMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}