
using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string ItemName,

    string? Description,

    [Range(
        typeof(decimal),
        "0.01",
        "1000000",
        ParseLimitsInInvariantCulture = true)]
    decimal Price
);
