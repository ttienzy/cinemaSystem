using Domain.Common;

namespace Domain.Entities.PromotionAggregate
{
    /// <summary>
    /// Promotion aggregate — supports percentage/fixed discounts with constraints.
    /// </summary>
    public class Promotion : BaseEntity, IAggregateRoot
    {
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public PromotionType Type { get; private set; }
        public decimal Value { get; private set; }             // percentage (0–100) or fixed VND
        public decimal? MaxDiscountAmount { get; private set; } // cap for percentage promotions
        public decimal? MinOrderValue { get; private set; }    // minimum spend to apply
        public int MaxUsageCount { get; private set; }         // 0 = unlimited
        public int CurrentUsageCount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core constructor
        private Promotion() { }

        public static Promotion Create(
            string code, string name, string? description,
            PromotionType type, decimal value,
            decimal? maxDiscountAmount, decimal? minOrderValue,
            int maxUsageCount,
            DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Promotion code is required.");
            if (endDate <= startDate)
                throw new DomainException("End date must be after start date.");
            if (type == PromotionType.Percentage && (value < 0 || value > 100))
                throw new DomainException("Percentage value must be between 0 and 100.");

            return new Promotion
            {
                Code = code.ToUpperInvariant(),
                Name = name,
                Description = description,
                Type = type,
                Value = value,
                MaxDiscountAmount = maxDiscountAmount,
                MinOrderValue = minOrderValue,
                MaxUsageCount = maxUsageCount,
                CurrentUsageCount = 0,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            };
        }

        /// <summary>Validate and calculate discount for a given order total.</summary>
        public (bool CanApply, decimal DiscountAmount, string Reason) Evaluate(decimal orderTotal)
        {
            if (!IsActive)
                return (false, 0, "Promotion is not active.");

            var now = DateTime.UtcNow;
            if (now < StartDate || now > EndDate)
                return (false, 0, "Promotion is not within valid date range.");

            if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
                return (false, 0, "Promotion usage limit reached.");

            if (MinOrderValue.HasValue && orderTotal < MinOrderValue.Value)
                return (false, 0, $"Minimum order value is {MinOrderValue.Value:N0} VND.");

            decimal discount = Type switch
            {
                PromotionType.Percentage => orderTotal * Value / 100,
                PromotionType.FixedAmount => Value,
                _ => 0
            };

            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
                discount = MaxDiscountAmount.Value;

            discount = Math.Min(discount, orderTotal); // can't discount more than total
            return (true, Math.Round(discount, 0), "Promotion applied successfully.");
        }

        public void IncrementUsage()
        {
            CurrentUsageCount++;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }

    public enum PromotionType
    {
        Percentage = 1,
        FixedAmount = 2
    }
}
