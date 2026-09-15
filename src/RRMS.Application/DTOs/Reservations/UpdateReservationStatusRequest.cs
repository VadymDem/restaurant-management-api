using RRMS.Domain.Enums;

namespace RRMS.Application.DTOs.Reservations;

public sealed record UpdateReservationStatusRequest(ReservationStatus Status);