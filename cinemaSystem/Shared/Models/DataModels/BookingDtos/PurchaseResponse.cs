using Domain.Entities.BookingAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.BookingDtos
{
    public class PurchaseResponse
    {
        public Guid BookingId { get; set; }
        public DateTime BookingTime { get; set; }
        public string MovieTitle { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalTickets { get; set; }
        public DateTime ShowTime { get; set; }
        public string CinemaName { get; set; }
        public BookingStatus Status { get; set; }
    }
}
