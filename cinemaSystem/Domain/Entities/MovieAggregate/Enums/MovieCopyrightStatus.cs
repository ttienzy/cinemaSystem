using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate.Enum
{
    public enum MovieCopyrightStatus
    {
        Active = 1, // The movie is currently under copyright protection
        Expired = 2, // The copyright for the movie has expired
        Pending = 3 // The copyright status is pending, possibly awaiting renewal or review
    }
}
