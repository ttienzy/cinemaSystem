using Domain.Entities.CinemaAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeSetupDataDto
    {
        public IEnumerable<ScreenInfoDto> Screens { get; set; } = new List<ScreenInfoDto>();
        public IEnumerable<SlotInfoDto> Slots { get; set; } = new List<SlotInfoDto>();
        public IEnumerable<PricingTierInfoDto> PricingTiers { get; set; } = new List<PricingTierInfoDto>();
        public IEnumerable<MovieInfoDto> Movies { get; set; } = new List<MovieInfoDto>();
        public IEnumerable<SeatTypeInfoDto> SeatTypes { get; set; } = new List<SeatTypeInfoDto>();
    }
    public class MovieInfoDto
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; } // Th?i lu?ng phim tính b?ng phút
    }

    public class ScreenInfoDto
    {
        public Guid ScreenId { get; set; }
        public string ScreenName { get; set; } = string.Empty;
        public ScreenType ScreenType { get; set; }  // Ví d?: 2D, 3D, IMAX
        public int SeatCapacity { get; set; }                   // T?ng s? gh?
        public bool IsActive { get; set; }                      // Còn ho?t d?ng hay không
    }
    public class SlotInfoDto
    {
        public Guid SlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SlotName { get; set; } = string.Empty;    // VD: "Bu?i sáng", "Bu?i t?i"
        public bool IsPeakTime { get; set; }                    // Gi? cao di?m hay không
    }
    public class PricingTierInfoDto
    {
        public Guid PricingTierId { get; set; }
        public string TierName { get; set; } = string.Empty;    // VD: "Standard", "Premium"
        public decimal Multiplier { get; set; }                 // H? s? giá vé
        public string Description { get; set; } = string.Empty; // Mô t? khung giá
    }
    public class SeatTypeInfoDto
    {
        public Guid SeatTypeId { get; set; }
        public string SeatTypeName { get; set; } = string.Empty; // VD: "Thu?ng", "VIP"
        public decimal Multiplier { get; set; }                 // H? s? giá vé cho lo?i gh?
    }
}
