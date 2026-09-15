using RRMS.Domain.Entities;

namespace RRMS.Application.Interfaces.Repositories;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    void Update(MenuItem menuItem);

    void Delete(MenuItem menuItem);
}