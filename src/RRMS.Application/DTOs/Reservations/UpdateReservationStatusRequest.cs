using System.ComponentModel.DataAnnotations;
using RRMS.Domain.Enums;

namespace RRMS.Application.DTOs.Reservations;

public sealed record UpdateReservationStatusRequest(
    [EnumDataType(typeof(ReservationStatus))] ReservationStatus Status);
