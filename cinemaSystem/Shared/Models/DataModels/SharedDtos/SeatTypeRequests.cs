using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DataModels.SharedDtos
{
    public class CreateSeatTypeRequest
    {
        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; } = string.Empty;

        [Range(0.1, 10.0)]
        public decimal PriceMultiplier { get; set; } = 1.0m;
    }

    public class UpdateSeatTypeRequest
    {
        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; } = string.Empty;

        [Range(0.1, 10.0)]
        public decimal PriceMultiplier { get; set; } = 1.0m;
    }
}
