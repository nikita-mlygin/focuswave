using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record FocusCycleStarted(Guid FocusCycleId, Guid UserId, DateTimeOffset StartedAt)
    : IDomainEvent
{
    public DateTimeOffset OccurredOn => StartedAt;
}
