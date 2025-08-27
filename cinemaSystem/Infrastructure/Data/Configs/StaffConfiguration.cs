using Domain.Entities.CinemaAggreagte;
using Domain.Entities.StaffAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.ToTable("Staffs");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.FullName).IsRequired(false).HasMaxLength(100);
            builder.Property(s => s.Position).IsRequired(false).HasMaxLength(100);
            builder.Property(s => s.Department).HasMaxLength(100);
            builder.Property(s => s.Phone).HasMaxLength(20);
            builder.Property(s => s.Email).IsRequired(false).HasMaxLength(100);
            builder.Property(s => s.Address).HasMaxLength(255);
            builder.Property(s => s.Salary).HasColumnType("decimal(18, 2)");

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Active = 0, OnLeave = 1, Terminated = 2

            builder.HasOne<Cinema>()
                .WithMany()
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Metadata.FindNavigation(nameof(Staff.Shifts))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
