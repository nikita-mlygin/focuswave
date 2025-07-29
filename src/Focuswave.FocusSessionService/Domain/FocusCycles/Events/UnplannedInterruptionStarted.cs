using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record UnplannedInterruptionStarted(Guid FocusCycleId, DateTimeOffset StartedAt)
    : IDomainEvent
{
    public DateTimeOffset OccurredOn => StartedAt;
}
