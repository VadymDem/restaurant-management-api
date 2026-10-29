using RRMS.Domain.Enums;

namespace RRMS.Application.DTOs.Reservations;

public sealed record ReservationResponse(
    long Id,

    Guid UserId,

    long TableId,

    DateOnly ReservationDate,

    TimeOnly ReservationTime,

    int GuestsCount,

    string? Notes,

    ReservationStatus Status
);
