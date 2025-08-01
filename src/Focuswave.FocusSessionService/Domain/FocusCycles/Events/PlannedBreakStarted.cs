using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record PlannedBreakStarted(
    Guid FocusCycleId,
    int Index,
    DateTimeOffset StartedAt,
    TimeSpan Duration
) : IDomainEvent
{
    public DateTimeOffset OccurredOn => StartedAt;
}
