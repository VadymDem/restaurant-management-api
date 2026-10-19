
namespace RRMS.Domain.Entities;

public class MenuItem
{
    public long Id { get; set; }

    public string ItemName { get; set; } = default!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Category { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
