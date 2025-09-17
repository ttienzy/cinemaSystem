using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate
{
    public class MovieCastCrew : BaseEntity
    {
        public Guid MovieId { get; private set; }
        public string? PersonName { get; private set; }
        public string? RoleType { get; private set; } // actor, director, writer

        public MovieCastCrew()
        {
        }
        public MovieCastCrew(Guid movieId, string personName, string roleType)
        {
            MovieId = movieId;
            PersonName = personName;
            RoleType = roleType;
        }
        public void UpdateDetails(Guid movieId, string personName, string roleType)
        {
            PersonName = personName;
            RoleType = roleType;
            MovieId = movieId;
        }
    }
}
