using RRMS.Application.Interfaces.Repositories;
using RRMS.Domain.Entities;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class RestaurantTableRepository : IRestaurantTableRepository
{
    private readonly AppDbContext _dbContext;

    public RestaurantTableRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<RestaurantTable>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: _dbContext.RestaurantTables.ToListAsync() ordered by Number.");

    public Task<RestaurantTable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return await _dbContext.RestaurantTables.FindAsync(...)");

    public Task<bool> NumberExistsAsync(int number, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: _dbContext.RestaurantTables.AnyAsync(t => t.Number == number)");

    public Task AddAsync(RestaurantTable table, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: await _dbContext.RestaurantTables.AddAsync(table);");

    public void Update(RestaurantTable table)
        => throw new NotImplementedException("TODO: _dbContext.RestaurantTables.Update(table);");

    public void Delete(RestaurantTable table)
        => throw new NotImplementedException("TODO: _dbContext.RestaurantTables.Remove(table);");
}