using RRMS.Application.DTOs.Reservations;
using RRMS.Domain.Enums;

namespace RRMS.Application.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReservationResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task CancelAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ReservationResponse> UpdateStatusAsync(Guid reservationId, ReservationStatus status, CancellationToken cancellationToken = default);
}