using Confluent.Kafka;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

public class FocusCycleStartedHandler(IProducer<string, FocusCycleEvent> kafkaProducer)
    : Common.DomainEvents.IEventHandler<FocusCycleStarted>
{
    private readonly IProducer<string, FocusCycleEvent> producer = kafkaProducer;

    public async Task HandleAsync(FocusCycleStarted e, CancellationToken ct)
    {
        var ev = new FocusCycleEvent
        {
            Base = IntegrationEventFactory.Create(e.OccurredOn),
            FocusCycleId = e.FocusCycleId.ToString(),
            EventTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                e.StartedAt.UtcDateTime
            ),
            EventType = FocusCycleEvent.Types.EventType.Start,
            FocusCycle = new FocusCycleDetail { UserId = e.UserId.ToString() },
        };

        await producer.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = e.FocusCycleId.ToString(), Value = ev },
            ct
        );
    }
}
