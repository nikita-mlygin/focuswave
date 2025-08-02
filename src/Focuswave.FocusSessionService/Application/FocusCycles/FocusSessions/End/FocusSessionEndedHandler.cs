using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.End;

public class FocusSessionEndedHandler(
    IProducer<string, FocusCycleEvent> kafka,
    ILogger<FocusSessionEndedHandler> logger
) : IEventHandler<FocusSessionEnded>
{
    public async Task HandleAsync(FocusSessionEnded e, CancellationToken ct)
    {
        var ev = FocusCycleEventFactory.CreateEndEvent(
            e.FocusCycleId,
            e.OccurredOn,
            FocusCycleEventFactory.GenerateDetail<FocusSessionDetail>(e.Index)
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
