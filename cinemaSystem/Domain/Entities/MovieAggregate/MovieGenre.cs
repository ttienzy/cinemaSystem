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
            Id = Guid.NewGuid();
        }
        public MovieGenre(Guid genreId)
        {
            Id = Guid.NewGuid();
            GenreId = genreId;
        }
        public void UpdateDetail(Guid genreId)
        {
            GenreId = genreId;
        }
    }
}
