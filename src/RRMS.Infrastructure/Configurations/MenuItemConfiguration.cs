
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RRMS.Domain.Entities;

namespace RRMS.Infrastructure.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems", table =>
        {
            table.HasCheckConstraint(
                "MenuItems_price_check",
                "price > 0");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(m => m.ItemName)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description");

        builder.Property(m => m.Price)
            .HasColumnName("price")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(m => m.Category)
            .HasColumnName("category");

        builder.Property(m => m.ImageUrl)
            .HasColumnName("image_url");

        builder.Property(m => m.IsAvailable)
            .HasColumnName("is_available")
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");
    }
}
