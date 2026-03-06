using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate.Enum
{
    public enum RatingStatus
    {
        P,    // General Audience
        K,    // Children under 13 must be accompanied by an adult
        T13,  // 13+
        T16,  // 16+
        T18,  // 18+
        C     // Banned
    }
}
