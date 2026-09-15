using RRMS.Domain.Enums;

namespace RRMS.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = default!;

    public string CustomerEmail { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public int NumberOfPeople { get; set; }

    public DateTime ReservationDateTime { get; set; }

    public string? SpecialRequests { get; set; }

    public ReservationStatus Status { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid TableId { get; set; }
    public RestaurantTable Table { get; set; } = default!;
}