using Domain.Entities.BookingAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.BookingDtos
{
    public class BookingCheckedInResponse
    {
        public DateTime BookingTime { get; set; }
        public string MovieTitle { get; set; }
        public string ScreenName { get; set; }
        public List<string> SeatsList { get; set; } = new List<string>();
        public decimal TotalAmount { get; set; }
        public decimal TotalTickets { get; set; }
        public string CinemaName { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public BookingStatus Status { get; set; }
        public bool IsCheckedIn { get; set; }
    }
}
