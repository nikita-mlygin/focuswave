using Confluent.Kafka;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Stop;

public class StopCycleEventHandler(IProducer<string, FocusCycleEvent> producer)
    : Common.DomainEvents.IEventHandler<FocusCycleStopped>
{
    readonly IProducer<string, FocusCycleEvent> producer = producer;

    public async Task HandleAsync(FocusCycleStopped de, CancellationToken ct)
    {
        var ie = FocusCycleEventFactory.CreateEndEvent(
            de.FocusCycleId,
            de.OccurredOn,
            FocusCycleEventFactory.GenerateDetail(de.UserId)
        );

        await producer.ProduceAsync(
            "focus-cycle-events",
            new() { Key = de.FocusCycleId.ToString(), Value = ie },
            ct
        );
    }
}
