namespace Focuswave.Common.DomainEvents;

public interface IEventDispatcher
{
    void Publish(IDomainEvent domainEvent);
    Task DispatchAsync(CancellationToken ct = default);
}
