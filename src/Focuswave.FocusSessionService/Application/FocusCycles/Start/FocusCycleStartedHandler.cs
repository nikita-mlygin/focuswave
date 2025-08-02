using Confluent.Kafka;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;
using Microsoft.Extensions.Logging;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

public class FocusCycleStartedHandler(
    IProducer<string, FocusCycleEvent> kafkaProducer,
    ILogger<FocusCycleStartedHandler> logger
) : Common.DomainEvents.IEventHandler<FocusCycleStarted>
{
    private readonly IProducer<string, FocusCycleEvent> producer = kafkaProducer;
    private readonly ILogger<FocusCycleStartedHandler> _logger = logger;

    public async Task HandleAsync(FocusCycleStarted de, CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling FocusCycleStarted event for FocusCycleId: {FocusCycleId}",
            de.FocusCycleId
        );

        var ev = FocusCycleEventFactory.CreateStartEventWithoutDuration(
            de.FocusCycleId,
            de.OccurredOn,
            FocusCycleEventFactory.GenerateDetail(de.UserId)
        );

        await producer.ProduceAsync(
            "focus-cycle-events",
            new Message<string, FocusCycleEvent> { Key = de.FocusCycleId.ToString(), Value = ev },
            ct
        );

        _logger.LogInformation(
            "Successfully produced Kafka message for FocusCycleId: {FocusCycleId}",
            de.FocusCycleId
        );
    }
}
