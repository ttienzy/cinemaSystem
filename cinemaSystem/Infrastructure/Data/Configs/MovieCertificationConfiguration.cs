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
    public class MovieCertificationConfiguration : IEntityTypeConfiguration<MovieCertification>
    {
        public void Configure(EntityTypeBuilder<MovieCertification> builder)
        {
            builder.ToTable("MovieCertifications");

            builder.HasKey(mc => mc.Id);

            builder.Property(mc => mc.CertificationBody).HasMaxLength(150);
            builder.Property(mc => mc.Rating).HasMaxLength(10);
        }
    }
}
