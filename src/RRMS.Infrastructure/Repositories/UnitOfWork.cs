using RRMS.Application.Interfaces.Repositories;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    private readonly Lazy<IUserRepository> _users;
    private readonly Lazy<IMenuItemRepository> _menuItems;
    private readonly Lazy<IRestaurantTableRepository> _restaurantTables;
    private readonly Lazy<IReservationRepository> _reservations;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _users = new Lazy<IUserRepository>(() => new UserRepository(dbContext));
        _menuItems = new Lazy<IMenuItemRepository>(() => new MenuItemRepository(dbContext));
        _restaurantTables = new Lazy<IRestaurantTableRepository>(() => new RestaurantTableRepository(dbContext));
        _reservations = new Lazy<IReservationRepository>(() => new ReservationRepository(dbContext));
    }

    public IUserRepository Users => _users.Value;

    public IMenuItemRepository MenuItems => _menuItems.Value;

    public IRestaurantTableRepository RestaurantTables => _restaurantTables.Value;

    public IReservationRepository Reservations => _reservations.Value;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}