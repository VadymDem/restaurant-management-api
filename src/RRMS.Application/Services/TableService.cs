using RRMS.Application.DTOs.Tables;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Application.Services;

public class TableService : ITableService
{
    // TODO: inject IRestaurantTableRepository and IUnitOfWork via constructor.

    public Task<IReadOnlyList<RestaurantTableResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return all available restaurant tables mapped to RestaurantTableResponse.");

    public Task<RestaurantTableResponse> CreateAsync(RestaurantTableRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: validate that the table number is not duplicated, create and save.");

    public Task<RestaurantTableResponse> UpdateAsync(Guid id, RestaurantTableRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: update an existing restaurant table or throw a not-found error.");

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: delete an existing restaurant table or throw a not-found error.");
}