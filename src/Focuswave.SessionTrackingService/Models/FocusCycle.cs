using System;

namespace Focuswave.SessionTrackingService.Models;

public class FocusCycle
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
}
