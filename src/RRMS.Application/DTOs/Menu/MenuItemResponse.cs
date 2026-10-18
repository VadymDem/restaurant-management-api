namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemResponse(
    long Id,
    string ItemName,
    string? Description,
    decimal Price);