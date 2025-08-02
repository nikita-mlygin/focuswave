using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.End;

public class PlannedBreakEndsEventHandler(
    IProducer<string, FocusCycleEvent> kafka,
    ILogger<PlannedBreakEndsEventHandler> logger
) : IEventHandler<PlannedBreakEnded>
{
    public async Task HandleAsync(PlannedBreakEnded e, CancellationToken ct)
    {
        var ev = FocusCycleEventFactory.CreateEndEvent(
            e.FocusCycleId,
            e.OccurredOn,
            FocusCycleEventFactory.GenerateDetail<PlannedBreakDetail>(e.Index)
        );

        await kafka.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = e.FocusCycleId.ToString(), Value = ev },
            ct
        );

        logger.LogInformation(
            "Successfully produced FocusSessionEnded event to Kafka for FocusCycleId: {FocusCycleId}",
            e.FocusCycleId
        );
    }
}
