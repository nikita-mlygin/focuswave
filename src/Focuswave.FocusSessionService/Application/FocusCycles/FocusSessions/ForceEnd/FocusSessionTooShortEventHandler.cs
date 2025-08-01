using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.ForceEnd;

public class FocusSessionTooShortEventHandler(IProducer<string, FocusCycleEvent> kafka)
    : IEventHandler<FocusSessionTooShortEvent>
{
    public async Task HandleAsync(FocusSessionTooShortEvent ed, CancellationToken ct)
    {
        var ev = FocusCycleEventFactory.CreateEndEvent(
            ed.FocusCycleId,
            ed.OccurredOn,
            FocusCycleEventFactory.GenerateDetail<FocusSessionTooShortDetail>(ed.Index)
        );

        await kafka.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = ed.FocusCycleId.ToString(), Value = ev },
            ct
        );
    }
}
