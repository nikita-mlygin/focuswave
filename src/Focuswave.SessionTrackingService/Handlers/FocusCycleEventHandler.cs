using Focuswave.Integration.Events;

namespace Focuswave.SessionTrackingService.Handlers;

public class FocusCycleEventHandler
{
    public async Task HandleAsync(
        FocusCycleEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(@event);
    }
}
