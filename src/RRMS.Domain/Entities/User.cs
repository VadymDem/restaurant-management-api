using RRMS.Domain.Enums;

namespace RRMS.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public UserRole Role { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
