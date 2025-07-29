using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record FocusCycleStopped(Guid FocusCycleId, Guid UserId, DateTimeOffset StoppedAt)
    : IDomainEvent
{
    public DateTimeOffset OccurredOn => StoppedAt;
}
