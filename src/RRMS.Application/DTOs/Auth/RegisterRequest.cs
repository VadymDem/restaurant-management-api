using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record RegisterRequest(
    [property: Required, MinLength(2), MaxLength(100)] string Name,
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    [property: Required, MinLength(8), MaxLength(100)] string Password);