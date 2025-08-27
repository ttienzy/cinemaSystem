using Domain.Entities.MovieAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class MovieCastCrewConfiguration : IEntityTypeConfiguration<MovieCastCrew>
    {
        public void Configure(EntityTypeBuilder<MovieCastCrew> builder)
        {
            builder.ToTable("MovieCastCrews");

            builder.HasKey(mcc => mcc.Id);

            builder.Property(mcc => mcc.PersonName).IsRequired().HasMaxLength(150);
            builder.Property(mcc => mcc.RoleType).IsRequired().HasMaxLength(50);

        }
    }
}
