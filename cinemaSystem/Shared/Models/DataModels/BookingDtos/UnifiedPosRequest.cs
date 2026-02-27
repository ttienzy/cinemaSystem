using System;
using System.Collections.Generic;

namespace Shared.Models.DataModels.BookingDtos
{
    /// <summary>
    /// Unified POS request: sells tickets + concessions in a single atomic transaction.
    /// Designed for high-throughput counter operations.
    /// </summary>
    public class UnifiedPosRequest
    {
        public Guid ShowtimeId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CounterSeatSelection> Seats { get; set; } = new();
        public List<ConcessionItemRequest> Concessions { get; set; } = new();
        public string? PromotionCode { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
    }

    public class ConcessionItemRequest
    {
        public Guid InventoryItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UnifiedPosResponse
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public Guid? ConcessionSaleId { get; set; }
        public decimal TicketAmount { get; set; }
        public decimal ConcessionAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookingTime { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
