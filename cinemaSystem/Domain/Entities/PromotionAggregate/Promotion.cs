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
        public int? MaxUsagePerUser { get; private set; }       // limit per user, null = unlimited
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }

        // Targeting constraints
        public Guid? SpecificMovieId { get; private set; }     // apply to specific movie
        public Guid? SpecificCinemaId { get; private set; }    // apply to specific cinema

        // Timestamps
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // EF Core constructor
        private Promotion() { }

        public static Promotion Create(
            string code, string name, string? description,
            PromotionType type, decimal value,
            decimal? maxDiscountAmount, decimal? minOrderValue,
            int maxUsageCount, int? maxUsagePerUser,
            DateTime startDate, DateTime endDate,
            Guid? specificMovieId = null, Guid? specificCinemaId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Promotion code is required.");
            if (endDate <= startDate)
                throw new DomainException("End date must be after start date.");
            if (type == PromotionType.Percentage && (value < 0 || value > 100))
                throw new DomainException("Percentage value must be between 0 and 100.");
            if (maxUsagePerUser.HasValue && maxUsagePerUser.Value <= 0)
                throw new DomainException("Max usage per user must be greater than 0.");

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
                MaxUsagePerUser = maxUsagePerUser,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                SpecificMovieId = specificMovieId,
                SpecificCinemaId = specificCinemaId,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>Validate and calculate discount for a given order total.</summary>
        public (bool CanApply, decimal DiscountAmount, string Reason) Evaluate(
            decimal orderTotal,
            Guid? movieId = null,
            Guid? cinemaId = null,
            Guid? userId = null,
            int userUsageCount = 0)
        {
            if (!IsActive)
                return (false, 0, "Promotion is not active.");

            var now = DateTime.UtcNow;
            if (now < StartDate || now > EndDate)
                return (false, 0, "Promotion is not within valid date range.");

            if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
                return (false, 0, "Promotion usage limit reached.");

            // Check specific movie constraint
            if (SpecificMovieId.HasValue && movieId.HasValue && movieId.Value != SpecificMovieId.Value)
                return (false, 0, "Promotion is not valid for this movie.");

            // Check specific cinema constraint
            if (SpecificCinemaId.HasValue && cinemaId.HasValue && cinemaId.Value != SpecificCinemaId.Value)
                return (false, 0, "Promotion is not valid for this cinema.");

            // Check per-user limit
            if (MaxUsagePerUser.HasValue && userUsageCount >= MaxUsagePerUser.Value)
                return (false, 0, "You have reached the maximum usage limit for this promotion.");

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

        /// <summary>Update promotion details (for admin management).</summary>
        public void UpdateDetails(
            string name, string? description,
            PromotionType type, decimal value,
            decimal? maxDiscountAmount, decimal? minOrderValue,
            int maxUsageCount, int? maxUsagePerUser,
            DateTime startDate, DateTime endDate,
            Guid? specificMovieId, Guid? specificCinemaId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Promotion name is required.");
            if (endDate <= startDate)
                throw new DomainException("End date must be after start date.");
            if (type == PromotionType.Percentage && (value < 0 || value > 100))
                throw new DomainException("Percentage value must be between 0 and 100.");
            if (maxUsagePerUser.HasValue && maxUsagePerUser.Value <= 0)
                throw new DomainException("Max usage per user must be greater than 0.");

            Name = name;
            Description = description;
            Type = type;
            Value = value;
            MaxDiscountAmount = maxDiscountAmount;
            MinOrderValue = minOrderValue;
            MaxUsageCount = maxUsageCount;
            MaxUsagePerUser = maxUsagePerUser;
            StartDate = startDate;
            EndDate = endDate;
            SpecificMovieId = specificMovieId;
            SpecificCinemaId = specificCinemaId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementUsage()
        {
            CurrentUsageCount++;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public void UpdateCode(string newCode)
        {
            if (string.IsNullOrWhiteSpace(newCode))
                throw new DomainException("Promotion code is required.");
            Code = newCode.ToUpperInvariant();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum PromotionType
    {
        Percentage = 1,
        FixedAmount = 2
    }
}
