using RRMS.Application.DTOs.Tables;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Entities;

namespace RRMS.Application.Services;

public class TableService : ITableService
{
    private readonly IRestaurantTableRepository _tableRepository;

    public TableService(IRestaurantTableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    // TODO: return all restaurant tables (CRUD listing) — separate task.
    public Task<IReadOnlyList<RestaurantTableResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: return all restaurant tables mapped to RestaurantTableResponse.");

    public async Task<IReadOnlyList<RestaurantTableResponse>> GetAvailableAsync(
        DateOnly? reservationDate,
        TimeOnly? reservationTime,
        int? guests,
        CancellationToken cancellationToken = default)
    {
        if (reservationDate.HasValue != reservationTime.HasValue)
        {
            throw new ArgumentException(
                "Both date and time must be provided to filter availability.");
        }

        if (guests is <= 0)
        {
            throw new ArgumentException(
                "Guests count must be greater than zero.");
        }

        var tables = await _tableRepository.GetAvailableAsync(
            reservationDate,
            reservationTime,
            guests,
            cancellationToken);

        return tables.Select(Map).ToList();
    }

    // TODO: create/update/delete — separate task.
    public Task<RestaurantTableResponse> CreateAsync(
        RestaurantTableRequest request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: validate that the table number is not duplicated, create and save.");

    public Task<RestaurantTableResponse> UpdateAsync(
        Guid id,
        RestaurantTableRequest request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: update an existing restaurant table or throw a not-found error.");

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: delete an existing restaurant table or throw a not-found error.");

    private static RestaurantTableResponse Map(RestaurantTable table)
        => new(
            table.Id,
            table.Number,
            table.Capacity,
            table.Location,
            table.IsActive);
}
