using RRMS.Domain.Enums;

namespace RRMS.Application.DTOs.Reservations;

public sealed record ReservationResponse(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string PhoneNumber,
    int NumberOfPeople,
    DateTime ReservationDateTime,
    string? SpecialRequests,
    Guid TableId,
    ReservationStatus Status);