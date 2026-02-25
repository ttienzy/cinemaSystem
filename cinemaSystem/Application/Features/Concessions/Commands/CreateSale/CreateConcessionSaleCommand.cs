using MediatR;

namespace Application.Features.Concessions.Commands.CreateSale
{
    public record CreateConcessionSaleCommand(
        Guid CinemaId,
        Guid StaffId,
        string PaymentMethod,
        Guid? BookingId,
        List<ConcessionItemInput> Items,
        string? PromotionCode) : IRequest<CreateConcessionSaleResult>;

    public record ConcessionItemInput(Guid InventoryItemId, int Quantity);

    public record CreateConcessionSaleResult(
        Guid SaleId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TotalAmount);
}
