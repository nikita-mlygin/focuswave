using Microsoft.Extensions.DependencyInjection;

namespace Focuswave.Common.DomainEvents;

public class InMemoryDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
{
    private readonly List<IDomainEvent> events = [];
    private readonly IServiceProvider serviceProvider = serviceProvider;

    public void Publish(IDomainEvent domainEvent)
    {
        events.Add(domainEvent);
    }

    public async Task DispatchAsync(CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync")!;
                await (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
            }
        }

        events.Clear();
    }
}
