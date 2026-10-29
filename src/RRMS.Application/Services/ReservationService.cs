using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Enums;

namespace RRMS.Application.Services;

public class ReservationService : IReservationService
{
    // TODO: inject IReservationRepository, IRestaurantTableRepository
    // and IUnitOfWork via constructor.

    public Task<ReservationResponse> CreateAsync(
        Guid userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: implement reservation creation.");

    public Task<IReadOnlyList<ReservationResponse>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: return reservations of the current user.");

    public Task CancelAsync(
        Guid userId,
        long reservationId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: cancel a reservation only if the current user owns it.");

    public Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: return all reservations for admin.");

    public Task<ReservationResponse> UpdateStatusAsync(
        long reservationId,
        ReservationStatus status,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "TODO: update reservation status.");
}
