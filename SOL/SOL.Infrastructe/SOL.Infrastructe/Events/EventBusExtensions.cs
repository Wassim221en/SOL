using System.Reflection;
using Template.Application.Common.Events;

namespace Template.Infrastructe.Events;

public static class EventBusExtensions
{
    public static void AutoSubscribeIntegrationEvents(
        this IEventBus eventBus,
        Assembly assembly)
    {
        var handlerInterface = typeof(IIntegrationEventHandler<>);

        var handlers = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { type, iface })
            .Where(x =>
                x.iface.IsGenericType &&
                x.iface.GetGenericTypeDefinition() == handlerInterface)
            .ToList();

        foreach (var handler in handlers)
        {
            var eventType = handler.iface.GetGenericArguments()[0];

            var subscribeMethod = typeof(IEventBus)
                .GetMethod(nameof(IEventBus.Subscribe))
                ?.MakeGenericMethod(eventType, handler.type);

            subscribeMethod?.Invoke(eventBus, null);
        }
    }
}