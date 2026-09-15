using Microsoft.Extensions.DependencyInjection;

namespace RRMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // TODO: register application services, e.g.:
        // services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IMenuService, MenuService>();
        // services.AddScoped<ITableService, TableService>();
        // services.AddScoped<IReservationService, ReservationService>();
        // services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}