using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieSectionSpecification : Specification<Movie, MovieSectionResponse>
    {
        public MovieSectionSpecification()
        {
            Query.AsNoTracking()
                .Select(e => new MovieSectionResponse
                {
                    MovieId = e.Id,
                    AgeRating = e.Rating,
                    Description = e.Description,
                    DurationMinutes = e.DurationMinutes,
                    PosterUrl = e.PosterUrl,
                    ReleaseDate = e.ReleaseDate,
                    Title = e.Title
                });
        }
    }
}
