using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Enums;

namespace RRMS.Application.Services;

public class ReservationService : IReservationService
{
    // TODO: inject IReservationRepository, IRestaurantTableRepository and IUnitOfWork via constructor.

    public Task<ReservationResponse> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: validate the table exists and is not already booked for the requested time, link the reservation to the authenticated user and save.");

    public Task<IReadOnlyList<ReservationResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return reservations of the current user.");

    public Task CancelAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: cancel a reservation only if the current user owns it; otherwise throw a forbidden error.");

    public Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO (admin): return all reservations.");

    public Task<ReservationResponse> UpdateStatusAsync(Guid reservationId, ReservationStatus status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO (admin): change the status of a reservation (Pending/Confirmed/Cancelled) and save.");
}