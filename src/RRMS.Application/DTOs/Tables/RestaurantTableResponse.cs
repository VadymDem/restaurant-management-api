namespace RRMS.Application.DTOs.Tables;

public sealed record RestaurantTableResponse(
    Guid Id,
    int Number,
    int Capacity);