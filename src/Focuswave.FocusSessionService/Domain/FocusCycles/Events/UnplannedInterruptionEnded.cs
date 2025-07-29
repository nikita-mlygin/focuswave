using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record UnplannedInterruptionEnded(Guid FocusCycleId, DateTimeOffset EndedAt) : IDomainEvent
{
    public DateTimeOffset OccurredOn => EndedAt;
}
