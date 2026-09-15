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

    public Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: _dbContext.MenuItems.ToListAsync() ordered by ItemName.");

    public Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return await _dbContext.MenuItems.FindAsync(...)");

    public Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: await _dbContext.MenuItems.AddAsync(menuItem);");

    public void Update(MenuItem menuItem)
        => throw new NotImplementedException("TODO: _dbContext.MenuItems.Update(menuItem);");

    public void Delete(MenuItem menuItem)
        => throw new NotImplementedException("TODO: _dbContext.MenuItems.Remove(menuItem);");
}