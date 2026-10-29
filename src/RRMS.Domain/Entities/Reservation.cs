using RRMS.Domain.Enums;

namespace RRMS.Domain.Entities;

public class Reservation
{
    public long Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public long TableId { get; set; }
    public RestaurantTable Table { get; set; } = default!;

    public DateOnly ReservationDate { get; set; }

    public TimeOnly ReservationTime { get; set; }

    public int GuestsCount { get; set; }

    public ReservationStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
