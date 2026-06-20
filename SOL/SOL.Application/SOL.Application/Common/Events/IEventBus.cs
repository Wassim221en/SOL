using Template.Domain.Events;

namespace Template.Application.Common.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
    void Subscribe<TEvent, THandler>() where TEvent : IntegrationEvent where THandler : IIntegrationEventHandler<TEvent>;
    void Unsubscribe<TEvent, THandler>() where TEvent : IntegrationEvent where THandler : IIntegrationEventHandler<TEvent>;
    void StartConsuming();
    void StopConsuming();
}
