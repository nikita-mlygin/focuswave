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
    public async Task HandleAsync(PlannedBreakStarted de, CancellationToken ct)
    {
        var ie = FocusCycleEventFactory.CreateStartEventWithDuration(
            de.FocusCycleId,
            de.OccurredOn,
            de.Duration,
            FocusCycleEventFactory.GenerateDetail<PlannedBreakDetail>(de.Index)
        );

        await kafka.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = de.FocusCycleId.ToString(), Value = ie },
            ct
        );
    }
}
