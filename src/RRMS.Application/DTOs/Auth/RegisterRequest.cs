using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record RegisterRequest(
    [param: Required, MinLength(2), MaxLength(100)] string Name,
    [param: Required, EmailAddress, MaxLength(255)] string Email,
    [param: Required, MinLength(8), MaxLength(100)] string Password);