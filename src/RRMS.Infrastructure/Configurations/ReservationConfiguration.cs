using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RRMS.Domain.Entities;

namespace RRMS.Infrastructure.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        // TODO: map table name (reservations), required columns, index on UserId and TableId,
        //       convert ReservationStatus enum to a string column, and set the relationship
        //       Reservation -> User (one-to-many) and Reservation -> RestaurantTable (one-to-many).
    }
}