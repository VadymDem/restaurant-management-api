using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Reservations;

public sealed record CreateReservationRequest(
    long TableId,

    DateOnly ReservationDate,

    TimeOnly ReservationTime,

    [property: Range(1, 100)]
    int GuestsCount,

    [property: MaxLength(1000)]
    string? Notes
);
