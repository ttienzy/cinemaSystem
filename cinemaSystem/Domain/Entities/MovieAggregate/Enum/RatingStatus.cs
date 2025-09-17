using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate.Enum
{
    public enum RatingStatus
    {
        P,    // Phổ biến
        K,    // Trẻ em
        T13,  // Trên 13 tuổi
        T16,  // Trên 16 tuổi  
        T18,  // Trên 18 tuổi
        C     // Cấm chiếu
    }
}
