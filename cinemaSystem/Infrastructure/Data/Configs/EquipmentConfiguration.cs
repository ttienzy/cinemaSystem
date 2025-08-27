using Domain.Entities.CinemaAggreagte;
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
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EquipmentType).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(50);

            builder.HasOne<Cinema>()
                .WithMany()
                .HasForeignKey(e => e.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Screen)
                .WithMany()
                .HasForeignKey(e => e.ScreenId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);


            builder.Metadata.FindNavigation(nameof(Equipment.MaintenanceLogs))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
