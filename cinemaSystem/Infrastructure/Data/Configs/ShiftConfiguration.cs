using Domain.Entities.CinemaAggregate;
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
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
        public void Configure(EntityTypeBuilder<Shift> builder)
        {
            builder.ToTable("Shifts");

            builder.HasKey(s => s.Id);

            builder.HasOne<Cinema>().WithMany().HasForeignKey(s => s.CinemaId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
