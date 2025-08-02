using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Focuswave.Common.DomainEvents;

public class InMemoryDispatcher(
    IServiceProvider serviceProvider,
    ILogger<InMemoryDispatcher> logger
) : IEventDispatcher
{
    private readonly List<IDomainEvent> events = [];
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<InMemoryDispatcher> logger = logger;

    public void Publish(IDomainEvent domainEvent)
    {
        logger.LogDebug("publishing event of type {EventType}", domainEvent.GetType().Name);
        events.Add(domainEvent);
    }

    public async Task DispatchAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("dispatching {Count} event(s)", events.Count);

        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = serviceProvider.GetServices(handlerType);

            logger.LogDebug(
                "dispatching event {EventType} to {HandlersCount} handler(s)",
                domainEvent.GetType().Name,
                handlers.Count()
            );

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync")!;
                try
                {
                    logger.LogDebug(
                        "handling event {EventType} with handler {HandlerType}",
                        domainEvent.GetType().Name,
                        handler.GetType().Name
                    );
                    await (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "error while handling event {EventType} with handler {HandlerType}",
                        domainEvent.GetType().Name,
                        handler.GetType().Name
                    );
                    throw;
                }
            }
        }

        events.Clear();
        logger.LogInformation("all events dispatched and cleared");
    }
}
