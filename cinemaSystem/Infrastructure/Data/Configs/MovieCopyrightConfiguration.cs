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
    public class MovieCopyrightConfiguration : IEntityTypeConfiguration<MovieCopyright>
    {
        public void Configure(EntityTypeBuilder<MovieCopyright> builder)
        {
            builder.ToTable("MovieCopyrights");

            builder.HasKey(mc => mc.Id);

            builder.Property(mc => mc.DistributorCompany).IsRequired(false).HasMaxLength(150);
            builder.Property(mc => mc.Status).IsRequired(false).HasMaxLength(50);
        }
    }
}
