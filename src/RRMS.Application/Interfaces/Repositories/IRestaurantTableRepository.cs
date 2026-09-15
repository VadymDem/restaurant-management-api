using RRMS.Domain.Entities;

namespace RRMS.Application.Interfaces.Repositories;

public interface IRestaurantTableRepository
{
    Task<IReadOnlyList<RestaurantTable>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RestaurantTable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NumberExistsAsync(int number, CancellationToken cancellationToken = default);

    Task AddAsync(RestaurantTable table, CancellationToken cancellationToken = default);

    void Update(RestaurantTable table);

    void Delete(RestaurantTable table);
}