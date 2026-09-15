namespace RRMS.Application.DTOs.Reservations;

public sealed record CreateReservationRequest(
    string CustomerName,
    string CustomerEmail,
    string PhoneNumber,
    int NumberOfPeople,
    DateTime ReservationDateTime,
    string? SpecialRequests,
    Guid TableId);