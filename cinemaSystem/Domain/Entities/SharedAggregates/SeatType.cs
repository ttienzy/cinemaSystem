using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SharedAggregates
{
    public class SeatType : BaseEntity, IAggregateRoot
    {
        public string TypeName { get; private set; } // e.g., Standard, VIP, Couple, Wheelchair
        public decimal PriceMultiplier { get; private set; }
        public SeatType()
        {
            Id = Guid.NewGuid();
        }
        public SeatType(string typeName, decimal priceMultiplier)
        {
            Id = Guid.NewGuid();
            TypeName = typeName;
            PriceMultiplier = priceMultiplier;
        }
        public void UpdateSeatType(string typeName, decimal priceMultiplier)
        {
            TypeName = typeName;
            PriceMultiplier = priceMultiplier;
        }
    }
}
