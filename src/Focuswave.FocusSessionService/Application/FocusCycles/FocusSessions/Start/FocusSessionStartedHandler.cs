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

    public async Task HandleAsync(FocusSessionStarted de, CancellationToken ct)
    {
        Console.WriteLine("Produce event");

        var ie = FocusCycleEventFactory.CreateStartEventWithDuration(
            de.FocusCycleId,
            de.OccurredOn,
            de.Duration,
            FocusCycleEventFactory.GenerateDetail<FocusSessionDetail>(de.Index)
        );

        await producer.ProduceAsync(
            topicName,
            new Message<string, FocusCycleEvent> { Key = de.FocusCycleId.ToString(), Value = ie },
            ct
        );
    }
}
