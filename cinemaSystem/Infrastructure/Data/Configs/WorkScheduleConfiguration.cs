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
    public class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
    {
        public void Configure(EntityTypeBuilder<WorkSchedule> builder)
        {
            builder.ToTable("WorkSchedules");
            builder.HasKey(ws => ws.Id);
            builder.Property(ws => ws.WorkDate)
               .HasColumnType("date")
               .IsRequired();
            builder.Property(ws => ws.ActualCheckInTime)
            .IsRequired(false);

            builder.HasOne(ws => ws.Staff)
            .WithMany(s => s.WorkSchedules)
            .HasForeignKey(ws => ws.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.Shift)
            .WithMany(st => st.WorkSchedules)
            .HasForeignKey(ws => ws.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
