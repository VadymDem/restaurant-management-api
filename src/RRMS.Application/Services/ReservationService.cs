using Microsoft.Extensions.Options;
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Application.Options;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IRestaurantTableRepository _tables;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<RestaurantOptions> _restaurantOptions;

    public ReservationService(
        IReservationRepository reservations,
        IRestaurantTableRepository tables,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IOptions<RestaurantOptions> restaurantOptions)
    {
        _reservations = reservations;
        _tables = tables;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _restaurantOptions = restaurantOptions;
    }

    public async Task<ReservationResponse> CreateAsync(
        Guid userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.GuestsCount <= 0)
            throw new ArgumentException(
                "Guest count must be greater than zero.");

        var restaurantTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            _restaurantOptions.Value.TimeZone);

        var currentRestaurantTime = TimeZoneInfo.ConvertTime(
            _timeProvider.GetUtcNow(),
            restaurantTimeZone).DateTime;

        var reservationDateTime =
            request.ReservationDate.ToDateTime(
                request.ReservationTime);

        if (reservationDateTime <= currentRestaurantTime)
        {
            throw new ArgumentException(
                "Reservation date and time must be in the future.");
        }

        if (request.TableId <= 0)
        {
            throw new ArgumentException(
                "Table ID must be greater than zero.");
        }

        var table = await _tables.GetByIdAsync(
            request.TableId, cancellationToken);

        if (table is null)
            throw new NotFoundException(
                "Restaurant table not found.");

        if (!table.IsActive)
            throw new ConflictException(
                "The selected table is inactive.");

        if (request.GuestsCount > table.Capacity)
            throw new ConflictException(
                "Guest count exceeds table capacity.");

        var isBooked = await _reservations.SlotExistsAsync(
            request.TableId,
            request.ReservationDate,
            request.ReservationTime,
            cancellationToken);

        if (isBooked)
            throw new ConflictException(
                "The selected table is already booked.");

        var reservation = new Reservation
        {
            UserId = userId,
            TableId = request.TableId,
            ReservationDate = request.ReservationDate,
            ReservationTime = request.ReservationTime,
            GuestsCount = request.GuestsCount,
            Notes = request.Notes,
            Status = ReservationStatus.Pending
        };

        await _reservations.AddAsync(
            reservation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new ReservationResponse(
            reservation.Id,
            reservation.UserId,
            reservation.TableId,
            reservation.ReservationDate,
            reservation.ReservationTime,
            reservation.GuestsCount,
            reservation.Notes,
            reservation.Status);
    }

    public Task<IReadOnlyList<ReservationResponse>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CancelAsync(
        Guid userId,
        long reservationId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ReservationResponse> UpdateStatusAsync(
        long reservationId,
        ReservationStatus status,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}