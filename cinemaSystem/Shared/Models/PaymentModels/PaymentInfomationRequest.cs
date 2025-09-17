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
        public required Guid UserId { get; set; }
        public required Guid ShowtimeId { get; set; }
        public Guid? BookingId { get; set; }
        public required List<SeatsSelectedResponse> SelectedSeats { get; set; }
    }
}
