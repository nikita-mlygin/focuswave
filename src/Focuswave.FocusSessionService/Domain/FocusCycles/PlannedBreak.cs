namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public record PlannedBreak(DateTimeOffset StartedAt, TimeSpan Duration);
