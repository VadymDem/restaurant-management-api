using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.DTOs.Auth;

public sealed record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password);