using System;
using System.Collections.Generic;

namespace Shared.Models.DataModels.BookingDtos
{
    public class CounterBookingRequest
    {
        public Guid ShowtimeId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CounterSeatSelection> Seats { get; set; } = new();
        public string? PromotionCode { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, BankTransfer, CreditCard
    }

    public class CounterSeatSelection
    {
        public Guid SeatId { get; set; }
        public decimal Price { get; set; }
    }

    public class CounterBookingResponse
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime BookingTime { get; set; }
    }
}
