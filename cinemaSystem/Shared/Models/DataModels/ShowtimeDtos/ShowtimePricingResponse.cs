namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimePricingResponse
    {
        public Guid Id { get; set; }
        public Guid SeatTypeId { get; set; }
        public string SeatTypeName { get; set; }
        public decimal FinalPrice { get; set; } = 0;
    }
}