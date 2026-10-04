using Microsoft.Extensions.DependencyInjection;
using RRMS.Application.Interfaces.Services;
using RRMS.Application.Services;

namespace RRMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}