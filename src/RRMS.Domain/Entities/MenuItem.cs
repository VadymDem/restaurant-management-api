namespace RRMS.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; }

    public string ItemName { get; set; } = default!;

    public string? Description { get; set; }

    public decimal Price { get; set; }
}