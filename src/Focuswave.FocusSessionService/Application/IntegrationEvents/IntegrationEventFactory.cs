using Focuswave.Integration;
using Google.Protobuf.WellKnownTypes;

namespace Focuswave.FocusSessionService.Application.IntegrationEvents;

public static class IntegrationEventFactory
{
    public const string ServiceSource = "FocusSessionService";

    public static IntegrationEvent Create(DateTimeOffset dt)
    {
        return new IntegrationEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            OccurredOn = Timestamp.FromDateTime(dt.UtcDateTime),
            Source = ServiceSource,
        };
    }
}
