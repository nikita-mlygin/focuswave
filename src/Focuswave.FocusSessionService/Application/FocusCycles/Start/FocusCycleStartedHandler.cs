using Confluent.Kafka;
using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration.Events;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

public class FocusCycleStartedHandler(IProducer<string, FocusCycleEvent> kafkaProducer)
    : Common.DomainEvents.IEventHandler<FocusCycleStarted>
{
    private readonly IProducer<string, FocusCycleEvent> producer = kafkaProducer;

    public async Task HandleAsync(FocusCycleStarted de, CancellationToken ct)
    {
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
    }
}
