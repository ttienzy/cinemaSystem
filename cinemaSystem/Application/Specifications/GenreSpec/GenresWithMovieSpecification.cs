using Ardalis.Specification;
using Domain.Entities.SharedAggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.GenreSpec
{
    public class GenresWithMovieSpecification : Specification<Genre>
    {
        public GenresWithMovieSpecification(List<Guid> genreIds)
        {
            Query
                .Where(genre => genreIds.Contains(genre.Id))
                .AsNoTracking();
        }
    }
}
