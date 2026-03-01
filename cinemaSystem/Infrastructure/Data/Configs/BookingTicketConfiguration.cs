using Domain.Entities.BookingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class BookingTicketConfiguration : IEntityTypeConfiguration<BookingTicket>
    {
        public void Configure(EntityTypeBuilder<BookingTicket> builder)
        {
            builder.ToTable("BookingTickets");

            builder.HasKey(bt => bt.Id);

            builder.Property(bt => bt.TicketPrice)
                .HasColumnType("decimal(18, 2)");

            builder.HasOne(bt => bt.Seat)
                .WithMany()
                .HasForeignKey(bt => bt.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Booking>()
                .WithMany(b => b.BookingTickets)
                .HasForeignKey(bt => bt.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
