using Domain.Entities.BookingAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class ConcessionSaleHistoryResponse
    {
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<ConcessionSaleHistoryItem> Items { get; set; } = new List<ConcessionSaleHistoryItem>();
        public TicketSaleHistoryResponse? TicketSales { get; set; }
    }
    public class TicketSaleHistoryResponse
    {
        public decimal TotalTickets { get; set; }
        public BookingStatus Status { get; set; }
    }
    public class ConcessionSaleHistoryItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
