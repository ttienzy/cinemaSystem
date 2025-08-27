using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> builder)
        {
            builder.ToTable("PricingTiers");

            builder.HasKey(pt => pt.Id);

            builder.Property(pt => pt.TierName).IsRequired().HasMaxLength(100);
            builder.Property(pt => pt.ValidDays).HasMaxLength(100);

            builder.Property(pt => pt.Multiplier)
                .HasColumnType("decimal(5, 2)");
        }
    }
}
