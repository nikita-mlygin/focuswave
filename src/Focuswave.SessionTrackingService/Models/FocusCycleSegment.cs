namespace Focuswave.SessionTrackingService.Models;

public class FocusCycleSegment
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public FocusCycle Cycle { get; set; } = null!;

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public FocusCycleSegmentType Type { get; set; }

    public Guid? FocusSessionId { get; set; }

    public TimeSpan? PlannedDuration { get; set; }
    public TimeSpan? ActualDuration => (EndedAt - StartedAt)?.Duration();
}
