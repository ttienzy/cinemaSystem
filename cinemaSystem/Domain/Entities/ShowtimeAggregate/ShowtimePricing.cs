using Domain.Common;
using Domain.Entities.SharedAggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.ShowtimeAggregate
{
    public class ShowtimePricing : BaseEntity
    {
        public Guid ShowtimeId { get; private set; }
        public Guid SeatTypeId { get; private set; } 
        public decimal BasePrice { get; private set; }
        public decimal FinalPrice { get; private set; }
        public ShowtimePricing()
        {
            Id = Guid.NewGuid();
        }
        public ShowtimePricing(Guid seatTypeId, decimal basePrice, decimal finalPrice)
        {
            Id = Guid.NewGuid();
            SeatTypeId = seatTypeId;
            BasePrice = basePrice;
            FinalPrice = finalPrice;
        }
        public void UpdatePricing(decimal basePrice, decimal finalPrice)
        {
            BasePrice = basePrice;
            FinalPrice = finalPrice;
        }

    }
}
