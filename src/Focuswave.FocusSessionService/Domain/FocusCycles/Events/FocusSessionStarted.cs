using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record FocusSessionStarted(Guid FocusCycleId, DateTimeOffset StartedAt, TimeSpan Duration)
    : IDomainEvent
{
    public DateTimeOffset OccurredOn => StartedAt;
}
