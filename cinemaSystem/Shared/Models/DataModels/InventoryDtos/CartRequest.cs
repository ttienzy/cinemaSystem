using Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class CartRequest
    {
        public TicketCartRequest? Tickets { get; set; }
        public IEnumerable<ConcessionCartRequest> Concessions { get; set; } = [];
        public string PaymentMethod { get; set; } = PaymentMethodConstants.Cash;
        public required Guid StaffId { get; set; }
    }
    public class TicketCartRequest
    {
        public Guid ShowTimeId { get; set; } = Guid.Empty;
        public IEnumerable<SelectedSeat> SelectedSeats { get; set; } = [];
    }
    public class ConcessionCartRequest
    {
        public Guid ItemId { get; set; } = Guid.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
    public class SelectedSeat
    {
        public Guid SeatId { get; set; } = Guid.Empty;
        public decimal Price { get; set; }
    }
}
