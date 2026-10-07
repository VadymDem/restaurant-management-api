using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record RegisterRequest(

    [Required, MinLength(2), MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);

