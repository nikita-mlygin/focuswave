namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public record FocusSession(DateTimeOffset StartedAt, TimeSpan Duration);
