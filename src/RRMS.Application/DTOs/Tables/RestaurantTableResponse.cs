namespace RRMS.Application.DTOs.Tables;

public sealed record RestaurantTableResponse(
    long Id,
    int Number,
    int Capacity,
    string? Location,
    bool IsActive);
