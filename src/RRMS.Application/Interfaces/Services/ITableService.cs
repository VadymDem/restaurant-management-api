using RRMS.Application.DTOs.Tables;

namespace RRMS.Application.Interfaces.Services;

public interface ITableService
{
    Task<IReadOnlyList<RestaurantTableResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestaurantTableResponse>> GetAvailableAsync(
        DateOnly? reservationDate,
        TimeOnly? reservationTime,
        int? guests,
        CancellationToken cancellationToken = default);

    Task<RestaurantTableResponse> CreateAsync(RestaurantTableRequest request, CancellationToken cancellationToken = default);

    Task<RestaurantTableResponse> UpdateAsync(Guid id, RestaurantTableRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}