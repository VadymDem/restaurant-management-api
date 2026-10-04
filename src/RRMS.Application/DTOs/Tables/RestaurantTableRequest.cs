using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Tables;

public sealed record RestaurantTableRequest(
    [property: Range(1, 10_000)] int Number,
    [property: Range(1, 1_000)] int Capacity);