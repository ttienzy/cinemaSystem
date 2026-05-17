using Cinema.Contracts.Events;
using Cinema.Contracts.Messaging;
using MassTransit;

namespace Cinema.Messaging.Configuration;

/// <summary>
/// Centralized event topology configuration for the Cinema system.
/// Maps all event types to their custom exchange names — called once,
/// eliminating the duplicated ConfigureEventTopology() across every service.
/// </summary>
public static class CinemaTopologyConfiguration
{
    /// <summary>
    /// Configures custom entity (exchange) names for all Cinema integration events.
    /// Must be called within UsingRabbitMq configuration.
    /// </summary>
    public static void ApplyCinemaTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<BookingCreatedEvent>(x => x.SetEntityName(CinemaEventNames.BookingCreated));
        cfg.Message<BookingCancelledEvent>(x => x.SetEntityName(CinemaEventNames.BookingCancelled));
        cfg.Message<BookingExpiredEvent>(x => x.SetEntityName(CinemaEventNames.BookingExpired));
        cfg.Message<PaymentCompletedEvent>(x => x.SetEntityName(CinemaEventNames.PaymentCompleted));
        cfg.Message<PaymentFailedEvent>(x => x.SetEntityName(CinemaEventNames.PaymentFailed));
    }
}
