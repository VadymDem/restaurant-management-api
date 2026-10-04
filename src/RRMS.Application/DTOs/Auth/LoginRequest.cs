using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);