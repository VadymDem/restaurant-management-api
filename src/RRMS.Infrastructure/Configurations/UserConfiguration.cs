using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RRMS.Domain.Entities;

namespace RRMS.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
{
    builder.ToTable("users");

    builder.HasKey(u => u.Id);
    builder.Property(u => u.Id).HasColumnName("id");

    builder.Property(u => u.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(u => u.Email)
        .HasColumnName("email")
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(u => u.PasswordHash)
        .HasColumnName("password_hash")
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(u => u.Role)
        .HasColumnName("role")
        .HasConversion<string>()
        .IsRequired()
        .HasMaxLength(20);

    builder.Property(u => u.CreatedAtUtc)
        .HasColumnName("created_at")
        .IsRequired();    

    builder.HasIndex(u => u.Email).IsUnique();
}
}