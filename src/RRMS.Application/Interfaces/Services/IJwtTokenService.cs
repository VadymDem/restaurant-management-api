using RRMS.Domain.Entities;

namespace RRMS.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}