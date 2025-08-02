using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;
using Microsoft.Extensions.Logging;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.ForceEnd;

public class FocusSessionTooShortEventHandler(
    IProducer<string, FocusCycleEvent> kafka,
    ILogger<FocusSessionTooShortEventHandler> logger
) : IEventHandler<FocusSessionTooShortEvent>
{
    public async Task HandleAsync(FocusSessionTooShortEvent ed, CancellationToken ct)
    {
        logger.LogInformation(
            "Handling FocusSessionTooShortEvent for FocusCycleId: {FocusCycleId}, Session Index: {SessionIndex}",
            ed.FocusCycleId,
            ed.Index
        );

        var ev = FocusCycleEventFactory.CreateEndEvent(
            ed.FocusCycleId,
            ed.OccurredOn,
            FocusCycleEventFactory.GenerateDetail<FocusSessionTooShortDetail>(ed.Index)
        );

        try
        {
            await kafka.ProduceAsync(
                "focus-cycle-events",
                new Message<string, FocusCycleEvent>
                {
                    Key = ed.FocusCycleId.ToString(),
                    Value = ev,
                },
                ct
            );
            logger.LogInformation(
                "Successfully produced FocusSessionTooShortEvent to Kafka for FocusCycleId: {FocusCycleId}",
                ed.FocusCycleId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error producing FocusSessionTooShortEvent to Kafka for FocusCycleId: {FocusCycleId}",
                ed.FocusCycleId
            );
            throw;
        }
    }
}
