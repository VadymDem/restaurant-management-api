
namespace RRMS.Domain.Entities;

public class RestaurantTable
{
    public long Id { get; set; }

    public int Number { get; set; }

    public int Capacity { get; set; }

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
