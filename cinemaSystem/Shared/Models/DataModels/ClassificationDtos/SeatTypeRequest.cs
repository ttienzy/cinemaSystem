using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ClassificationDtos
{
    public class SeatTypeRequest
    {
        public required string TypeName { get; set; } 
        public required decimal PriceMultiplier { get; set; }
    }
}
