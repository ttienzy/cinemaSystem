using System;

namespace Shared.Models.DataModels.SharedDtos
{
    public class SeatTypeDto
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal PriceMultiplier { get; set; }
    }
}
