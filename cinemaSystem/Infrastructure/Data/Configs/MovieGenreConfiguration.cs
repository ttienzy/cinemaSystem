using Domain.Entities.MovieAggregate;
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
    public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
    {
        public void Configure(EntityTypeBuilder<MovieGenre> builder)
        {
            builder.ToTable("MovieGenres");

            builder.HasKey(mg => mg.Id);


            builder.HasOne<Genre>()
                .WithMany()
                .HasForeignKey(mg => mg.GenreId);
        }
    }
}
