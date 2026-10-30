using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(255)] string Email,
    [param: Required, MaxLength(100)] string Password);
