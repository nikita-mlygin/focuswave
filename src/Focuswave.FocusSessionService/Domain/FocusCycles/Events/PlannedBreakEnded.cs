using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record PlannedBreakEnded(Guid FocusCycleId, int Index, DateTimeOffset EndedAt) : IDomainEvent
{
    public DateTimeOffset OccurredOn => EndedAt;
}
