using RRMS.Application.DTOs.Tables;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Entities;
using RRMS.Application.Exceptions;

namespace RRMS.Application.Services;

public class TableService : ITableService
{
    private readonly IRestaurantTableRepository _tableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TableService(
        IRestaurantTableRepository tableRepository,
        IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<RestaurantTableResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var tables = await _tableRepository.GetAllAsync(cancellationToken);
        return tables.Select(Map).ToList();
    }

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

    public async Task<RestaurantTableResponse> CreateAsync(
        RestaurantTableRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (await _tableRepository.NumberExistsAsync(
                request.Number, cancellationToken))
        {
            throw new ConflictException(
                $"Table number {request.Number} already exists.");
        }

        var table = new RestaurantTable
        {
            Number = request.Number,
            Capacity = request.Capacity,
            IsActive = true
        };

        await _tableRepository.AddAsync(table, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(table);
    }

    public async Task<RestaurantTableResponse> UpdateAsync(
        long id,
        RestaurantTableRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var table = await _tableRepository.GetByIdAsync(
            id, cancellationToken);

        if (table is null)
        {
            throw new NotFoundException(
                 $"Table with ID {id} was not found.");
        }

        if (table.Number != request.Number &&
            await _tableRepository.NumberExistsAsync(
                request.Number, cancellationToken))
        {
            throw new ConflictException(
                $"Table number {request.Number} already exists.");
        }

        table.Number = request.Number;
        table.Capacity = request.Capacity;

        _tableRepository.Update(table);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(table);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(
            id, cancellationToken);

        if (table is null)
        {
            throw new NotFoundException(
                $"Table with ID {id} was not found.");
        }

        table.IsActive = false;

        _tableRepository.Update(table);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(RestaurantTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Number is < 1 or > 10_000)
        {
            throw new ArgumentException(
                "Table number must be between 1 and 10000.");
        }

        if (request.Capacity is < 1 or > 1_000)
        {
            throw new ArgumentException(
                "Table capacity must be between 1 and 1000.");
        }
    }

    private static RestaurantTableResponse Map(RestaurantTable table)
        => new(
            table.Id,
            table.Number,
            table.Capacity,
            table.Location,
            table.IsActive);
}
