using Domain.Entities.BookingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs
{
    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RefundAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(r => r.RefundPercentage)
                .HasColumnType("decimal(18, 2)");

            builder.Property(r => r.Reason)
                .HasMaxLength(500);

            builder.HasOne<Booking>()
                .WithMany(b => b.Refunds)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
