using Focuswave.Common.DomainEvents;

namespace Focuswave.FocusSessionService.Domain.FocusCycles.Events;

public record FocusSessionEnded(Guid FocusCycleId, DateTimeOffset EndedAt) : IDomainEvent
{
    public DateTimeOffset OccurredOn => EndedAt;
}
