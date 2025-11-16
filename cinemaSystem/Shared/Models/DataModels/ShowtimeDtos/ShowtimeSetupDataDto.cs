using Domain.Entities.CinemaAggreagte.Enum;
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
        public int Duration { get; set; } // Thời lượng phim tính bằng phút
    }

    public class ScreenInfoDto
    {
        public Guid ScreenId { get; set; }
        public string ScreenName { get; set; } = string.Empty;
        public ScreenType ScreenType { get; set; }  // Ví dụ: 2D, 3D, IMAX
        public int SeatCapacity { get; set; }                   // Tổng số ghế
        public bool IsActive { get; set; }                      // Còn hoạt động hay không
    }
    public class SlotInfoDto
    {
        public Guid SlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SlotName { get; set; } = string.Empty;    // VD: "Buổi sáng", "Buổi tối"
        public bool IsPeakTime { get; set; }                    // Giờ cao điểm hay không
    }
    public class PricingTierInfoDto
    {
        public Guid PricingTierId { get; set; }
        public string TierName { get; set; } = string.Empty;    // VD: "Standard", "Premium"
        public decimal Multiplier { get; set; }                 // Hệ số giá vé
        public string Description { get; set; } = string.Empty; // Mô tả khung giá
    }
    public class SeatTypeInfoDto
    {
        public Guid SeatTypeId { get; set; }
        public string SeatTypeName { get; set; } = string.Empty; // VD: "Thường", "VIP"
        public decimal Multiplier { get; set; }                 // Hệ số giá vé cho loại ghế
    }
}
