namespace RRMS.Application.DTOs.Menu;

public sealed record MenuItemRequest(
    string ItemName,
    string? Description,
    decimal Price);