using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Redis.Constants
{
    public class CacheKey
    {
        public static string RefreshToken(Guid userId) => $"refreshtoken:{userId}:user";
        public static string SeatingPlan(Guid showtimeId) => $"showtime:{showtimeId}:seating-plan";
    }
}
