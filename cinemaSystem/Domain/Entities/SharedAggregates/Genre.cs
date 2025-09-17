using Domain.Common;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SharedAggregates
{
    public class Genre :BaseEntity, IAggregateRoot
    {
        public string? GenreName { get; private set; } // e.g., Action, Comedy, Horror
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public Genre()
        {
            IsActive = true;
        }
        public Genre(string genreName, string? description)
        {
            GenreName = genreName;
            Description = description;
            IsActive = true;
        }
        public void UpdateGenre(string genreName, string? description = null)
        {
            GenreName = genreName;
            Description = description;
        }
    }
}
