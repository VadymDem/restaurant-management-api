using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RRMS.Domain.Entities;

namespace RRMS.Infrastructure.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.ItemName)
            .HasColumnName("item_name")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Description)
            .HasColumnName("description");

        builder.Property(m => m.Price)
            .HasColumnName("price")
            .HasPrecision(10, 2)
            .IsRequired();
    }
}
