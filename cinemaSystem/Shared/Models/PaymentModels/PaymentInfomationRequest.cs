using Shared.Models.DataModels.BookingDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.PaymentModels
{
    public class PaymentInfomationRequest
    {
        public Guid UserId { get; set; }
        public Guid ShowtimeId { get; set; }
        public Guid? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SeatsSelectedResponse> SelectedSeats { get; set; }
    }
}
