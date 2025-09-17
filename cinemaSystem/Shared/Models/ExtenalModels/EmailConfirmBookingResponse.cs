using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.ExtenalModels
{
    public class EmailConfirmBookingResponse
    {
        public string MovieTitle { get; set; }
        public DateTime Showtime { get; set; }
        public string TimeSlot { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalAmount { get; set; }
        public string ScreenName { get; set; }
        public string CinemaName { get; set; }
        public List<string> SeatsList { get; set; }
    }
}
