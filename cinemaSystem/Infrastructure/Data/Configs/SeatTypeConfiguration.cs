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
    public class SeatTypeConfiguration : IEntityTypeConfiguration<SeatType>
    {
        public void Configure(EntityTypeBuilder<SeatType> builder)
        {
            builder.ToTable("SeatTypes");

            builder.HasKey(st => st.Id);

            builder.Property(st => st.TypeName).IsRequired().HasMaxLength(50);

            builder.Property(st => st.PriceMultiplier)
                .HasColumnType("decimal(5, 2)");
        }
    }
}
