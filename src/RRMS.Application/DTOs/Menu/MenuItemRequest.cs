using System.ComponentModel.DataAnnotations;
using RRMS.Application.Validation;

namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemRequest
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string ItemName { get; init; } = default!;

    public string? Description { get; init; }

    [Range(
        typeof(decimal),
        "0.01",
        "1000000",
        ParseLimitsInInvariantCulture = true)]
    [DecimalPlaces(2)]
    public decimal Price { get; init; }

    [StringLength(50)]
    public string? Category { get; init; }

    [StringLength(500)]
    public string? ImageUrl { get; init; }

    public bool IsAvailable { get; init; } = true;
}
