using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieBaseSpecification : Specification<Movie, MovieBaseResponse>
    {
        public MovieBaseSpecification()
        {
            Query.AsNoTracking().Select(m => new MovieBaseResponse
                { MovieId = m.Id, Title = m.Title });
        }
    }
}
