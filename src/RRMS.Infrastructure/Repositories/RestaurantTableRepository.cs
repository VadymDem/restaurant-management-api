using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<RestaurantTable>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RestaurantTables
            .AsNoTracking()
            .OrderBy(t => t.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<RestaurantTable?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RestaurantTables
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> NumberExistsAsync(
        int number,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RestaurantTables
            .AnyAsync(t => t.Number == number, cancellationToken);
    }

    public async Task AddAsync(
        RestaurantTable table,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RestaurantTables.AddAsync(
            table, cancellationToken);
    }

    public void Update(RestaurantTable table)
    {
        _dbContext.RestaurantTables.Update(table);
    }

    public void Delete(RestaurantTable table)
    {
        _dbContext.RestaurantTables.Remove(table);
    }
}
