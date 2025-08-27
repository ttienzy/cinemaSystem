using Domain.Entities.EquipmentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class MaintenanceLogConfiguration : IEntityTypeConfiguration<MaintenanceLog>
    {
        public void Configure(EntityTypeBuilder<MaintenanceLog> builder)
        {
            builder.ToTable("MaintenanceLogs");

            builder.HasKey(ml => ml.Id);

            builder.Property(ml => ml.Cost)
                .HasColumnType("decimal(18, 2)")
                .IsRequired(false);

            builder.Property(ml => ml.IssuesFound).HasMaxLength(1000);
        }
    }
}
