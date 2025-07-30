using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.Start;

// event handler
public class PlannedBreakStartedHandler(IProducer<string, FocusCycleEvent> kafka)
    : IEventHandler<PlannedBreakStarted>
{
    public async Task HandleAsync(PlannedBreakStarted e, CancellationToken ct)
    {
        var ev = new FocusCycleEvent
        {
            Base = IntegrationEventFactory.Create(e.StartedAt),
            FocusCycleId = e.FocusCycleId.ToString(),
            EventTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                e.StartedAt.UtcDateTime
            ),
            EventType = FocusCycleEvent.Types.EventType.Start,
            PlannedBreak = new()
            {
                Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(e.Duration),
            },
        };

        await kafka.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = e.FocusCycleId.ToString(), Value = ev },
            ct
        );
    }
}
