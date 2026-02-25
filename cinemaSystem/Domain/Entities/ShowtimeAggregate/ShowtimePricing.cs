using Domain.Common;

namespace Domain.Entities.ShowtimeAggregate
{
    /// <summary>
    /// ShowtimePricing — links a seat type to its price for a specific showtime.
    /// Enhanced with ScreenSurcharge (e.g., IMAX, 3D surcharge) and Calculate() factory.
    /// </summary>
    public class ShowtimePricing : BaseEntity
    {
        public Guid ShowtimeId { get; private set; }
        public Guid SeatTypeId { get; private set; }
        public decimal BasePrice { get; private set; }
        public decimal ScreenSurcharge { get; private set; }
        public decimal FinalPrice { get; private set; }

        // EF Core constructor
        private ShowtimePricing() { }

        /// <summary>
        /// Factory method: calculates final price from base, seat multiplier, tier, and screen surcharge.
        /// </summary>
        public static ShowtimePricing Calculate(
            Guid seatTypeId,
            decimal basePrice,
            decimal seatTypeMultiplier,
            decimal pricingTierMultiplier,
            decimal screenSurcharge = 0)
        {
            var calculated = basePrice * seatTypeMultiplier * pricingTierMultiplier + screenSurcharge;
            return new ShowtimePricing
            {
                SeatTypeId = seatTypeId,
                BasePrice = basePrice,
                ScreenSurcharge = screenSurcharge,
                FinalPrice = Math.Round(calculated, 0) // VND has no decimals
            };
        }

        // Legacy constructor (backward compatibility)
        public ShowtimePricing(Guid seatTypeId, decimal basePrice, decimal finalPrice)
        {
            SeatTypeId = seatTypeId;
            BasePrice = basePrice;
            FinalPrice = finalPrice;
            ScreenSurcharge = 0;
        }

        public void UpdatePricing(decimal basePrice, decimal finalPrice)
        {
            BasePrice = basePrice;
            FinalPrice = finalPrice;
        }

        public void SetScreenSurcharge(decimal surcharge)
        {
            ScreenSurcharge = surcharge;
        }
    }
}
