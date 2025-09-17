using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.BookingDtos
{
    public class CleanBookingModel
    {
        public Guid Key { get; set; }
        public HashSet<Guid> Values { get; set; } = new HashSet<Guid>();
    }
}
