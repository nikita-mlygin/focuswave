namespace Focuswave.Common.DomainEvents;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
