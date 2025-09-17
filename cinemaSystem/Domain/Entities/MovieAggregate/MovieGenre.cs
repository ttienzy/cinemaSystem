using Domain.Common;
using Domain.Entities.SharedAggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate
{
    public class MovieGenre : BaseEntity
    {
        public Guid MovieId { get; private set; }
        public Guid GenreId { get; private set; }

        public MovieGenre()
        {
        }
        public MovieGenre(Guid genreId, Guid movieId)
        {
            MovieId = movieId;
            GenreId = genreId;
        }
        public void UpdateDetail(Guid genreId)
        {
            GenreId = genreId;
        }
    }
}
