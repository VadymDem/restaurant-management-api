using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Infrastructure.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", "public");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.TableId)
            .HasColumnName("table_id")
            .IsRequired();

        builder.Property(r => r.ReservationDate)
            .HasColumnName("reservation_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(r => r.ReservationTime)
            .HasColumnName("reservation_time")
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(r => r.GuestsCount)
            .HasColumnName("guests_count")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReservationStatus.Pending)
            .IsRequired();

        builder.Property(r => r.Notes)
            .HasColumnName("notes");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_reservations_user_id");

        builder.HasIndex(r => r.TableId)
            .HasDatabaseName("ix_reservations_table_id");

        builder.HasIndex(r => r.ReservationDate)
            .HasDatabaseName("ix_reservations_date");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Table)
            .WithMany()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
