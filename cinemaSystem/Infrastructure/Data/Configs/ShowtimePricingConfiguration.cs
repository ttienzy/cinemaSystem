using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class ShowtimePricingConfiguration : IEntityTypeConfiguration<ShowtimePricing>
    {
        public void Configure(EntityTypeBuilder<ShowtimePricing> builder)
        {
            builder.ToTable("ShowtimePricings");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.BasePrice).HasColumnType("decimal(18, 2)");
            builder.Property(sp => sp.FinalPrice).HasColumnType("decimal(18, 2)");
            builder.HasOne<SeatType>().WithMany().HasForeignKey(sp => sp.SeatTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
