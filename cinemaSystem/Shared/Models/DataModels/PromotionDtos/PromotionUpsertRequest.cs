using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DataModels.PromotionDtos
{
    /// <summary>
    /// Request DTO for creating or updating a promotion.
    /// </summary>
    public class PromotionUpsertRequest
    {
        [Required]
        [StringLength(50)]
        public required string Code { get; set; }

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string Type { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MaxUsageCount { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUsagePerUser { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Targeting constraints
        public Guid? SpecificMovieId { get; set; }
        public Guid? SpecificCinemaId { get; set; }
    }

    /// <summary>
    /// Response DTO for promotion details.
    /// </summary>
    public class PromotionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int MaxUsageCount { get; set; }
        public int CurrentUsageCount { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Guid? SpecificMovieId { get; set; }
        public Guid? SpecificCinemaId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
