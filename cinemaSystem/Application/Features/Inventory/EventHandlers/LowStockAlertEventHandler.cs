using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Inventory.EventHandlers
{
    /// <summary>
    /// Handles LowStockAlertEvent:
    /// - Logs critical warning
    /// - Future: Send email/SMS alert to inventory manager
    /// - Future: Auto-create restock order
    /// </summary>
    public class LowStockAlertEventHandler(
        ILogger<LowStockAlertEventHandler> logger) : INotificationHandler<LowStockAlertEvent>
    {
        public Task Handle(LowStockAlertEvent e, CancellationToken ct)
        {
            logger.LogCritical(
                "⚠ LOW STOCK ALERT: Item '{ItemName}' (ID: {ItemId}) " +
                "has {Current} units remaining (minimum: {Minimum}).",
                e.ItemName, e.InventoryItemId, e.CurrentStock, e.MinimumStock);

            // TODO: Send email to inventory manager
            // TODO: Send SignalR notification to dashboard
            // TODO: Create auto-restock task

            return Task.CompletedTask;
        }
    }
}
