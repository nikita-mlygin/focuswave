using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record UnplannedInterruptionStarted(Guid FocusCycleId, int Index, DateTimeOffset StartedAt)
    : IDomainEvent
{
    public DateTimeOffset OccurredOn => StartedAt;
}
