using Confluent.Kafka;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using Focuswave.Integration;
using Focuswave.Integration.Events;
using Google.Protobuf.WellKnownTypes;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.Start;

public class FocusSessionStartedHandler(IProducer<string, FocusCycleEvent> kafkaProducer)
    : IEventHandler<FocusSessionStarted>
{
    private readonly IProducer<string, FocusCycleEvent> producer = kafkaProducer;
    private readonly string topicName = "focus-cycle-events"; // TODO

    public async Task HandleAsync(FocusSessionStarted domainEvent, CancellationToken ct)
    {
        Console.WriteLine("Produce event");

        var integrationEvent = new FocusCycleEvent
        {
            Base = new IntegrationEvent
            {
                EventId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                OccurredOn = Timestamp.FromDateTime(domainEvent.OccurredOn.UtcDateTime),
                Source = "FocusSessionService",
            },
            FocusCycleId = domainEvent.FocusCycleId.ToString(),
            EventTime = Timestamp.FromDateTime(domainEvent.StartedAt.UtcDateTime),
            EventType = FocusCycleEvent.Types.EventType.Start,
            FocusSession = new FocusSessionDetail
            {
                Duration = Duration.FromTimeSpan(domainEvent.Duration),
            },
        };

        await producer.ProduceAsync(
            topicName,
            new Message<string, FocusCycleEvent>
            {
                Key = domainEvent.FocusCycleId.ToString(),
                Value = integrationEvent,
            },
            ct
        );
    }
}
