using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;
using Microsoft.Extensions.Logging;

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.Start;

// event handler
public class PlannedBreakStartedHandler(
    IProducer<string, FocusCycleEvent> kafka,
    ILogger<PlannedBreakStartedHandler> logger
) : IEventHandler<PlannedBreakStarted>
{
    private readonly ILogger<PlannedBreakStartedHandler> _logger = logger;

    public async Task HandleAsync(PlannedBreakStarted de, CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling PlannedBreakStarted event for FocusCycleId: {FocusCycleId}",
            de.FocusCycleId
        );

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

        _logger.LogInformation(
            "Successfully produced Kafka message for PlannedBreakStarted event for FocusCycleId: {FocusCycleId}",
            de.FocusCycleId
        );
    }
}
