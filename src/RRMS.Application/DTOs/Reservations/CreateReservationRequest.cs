using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Reservations;

public sealed record CreateReservationRequest(
    [property: Required, MaxLength(100)] string CustomerName,
    [property: Required, EmailAddress, MaxLength(255)] string CustomerEmail,
    [property: Required, Phone, MaxLength(30)] string PhoneNumber,
    [property: Range(1, 100)] int NumberOfPeople,
    DateTime ReservationDateTime,
    [property: MaxLength(1000)] string? SpecialRequests,
    Guid TableId);