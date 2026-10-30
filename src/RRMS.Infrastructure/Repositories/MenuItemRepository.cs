
using Microsoft.EntityFrameworkCore;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Domain.Entities;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly AppDbContext _dbContext;

    public MenuItemRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MenuItem>> GetAllAsync(
        bool includeUnavailable,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuItems
            .AsNoTracking()
            .Where(m => includeUnavailable || m.IsAvailable);

        return await query
            .OrderBy(m => m.ItemName)
            .ToListAsync(cancellationToken);
    }

    public Task<MenuItem?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
        => _dbContext.MenuItems.SingleOrDefaultAsync(
            m => m.Id == id, cancellationToken);

    public async Task AddAsync(
        MenuItem menuItem,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.MenuItems.AddAsync(menuItem, cancellationToken);
    }

    public void Update(MenuItem menuItem)
        => _dbContext.MenuItems.Update(menuItem);

    public void Delete(MenuItem menuItem)
        => _dbContext.MenuItems.Remove(menuItem);
}