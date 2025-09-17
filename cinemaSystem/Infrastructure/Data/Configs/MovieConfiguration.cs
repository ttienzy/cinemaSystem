using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToTable("Movies");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Description).HasMaxLength(2000);
            builder.Property(m => m.PosterUrl).HasMaxLength(500);
            builder.Property(m => m.Trailer).HasMaxLength(500).HasDefaultValue("No information yet");

            builder.Property(m => m.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Upcoming = 0, NowShowing = 1, Archived = 2

            builder.Property(m => m.Rating)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>()
                .HasDefaultValue(RatingStatus.P);

            builder.Metadata.FindNavigation(nameof(Movie.Copyrights))?.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Movie.Certifications))?.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Movie.CastCrew))?.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Movie.MovieGenres))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
