namespace RRMS.Application.DTOs.Tables;

public sealed record RestaurantTableRequest(
    int Number,
    int Capacity);