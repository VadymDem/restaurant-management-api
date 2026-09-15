using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: register the EF Core DbContext against Supabase PostgreSQL, e.g.:
        // services.AddDbContext<AppDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection")));

        // TODO: register repositories (IUserRepository, IMenuItemRepository,
        //       IRestaurantTableRepository, IReservationRepository) and IUnitOfWork as scoped.

        return services;
    }
}