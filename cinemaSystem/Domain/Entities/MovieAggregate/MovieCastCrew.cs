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
            Id = Guid.NewGuid();
        }
        public MovieCastCrew(string personName, string roleType)
        {
            Id = Guid.NewGuid();
            PersonName = personName;
            RoleType = roleType;
        }
        public void UpdateDetails(string personName, string roleType)
        {
            PersonName = personName;
            RoleType = roleType;
        }
    }
}
