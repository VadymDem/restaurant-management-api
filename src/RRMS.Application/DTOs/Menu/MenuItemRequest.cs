using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemRequest(
    [property: Required, MaxLength(100)] string ItemName,
    [property: MaxLength(500)] string? Description,
    [property: Range(0.01, 1_000_000.00)] decimal Price);