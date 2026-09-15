namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemResponse(
    Guid Id,
    string ItemName,
    string? Description,
    decimal Price);