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
        var ie = new FocusCycleEvent()
        {
            Base = IntegrationEventFactory.Create(de.StoppedAt),
            EventType = FocusCycleEvent.Types.EventType.End,
            FocusCycle = new() { UserId = de.UserId.ToString() },
            EventTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                de.StoppedAt.UtcDateTime
            ),
            FocusCycleId = de.FocusCycleId.ToString(),
        };

        await producer.ProduceAsync(
            "focus-cycle-events",
            new() { Key = de.FocusCycleId.ToString(), Value = ie },
            ct
        );
    }
}
