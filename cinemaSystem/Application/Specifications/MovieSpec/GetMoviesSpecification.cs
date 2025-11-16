using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class GetMoviesSpecification : Specification<Movie, MovieInfoDto>
    {
        public GetMoviesSpecification()
        {
            Query.AsNoTracking()
                .Select(m => new MovieInfoDto
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    Duration = m.DurationMinutes
                });
        }
    }
}
