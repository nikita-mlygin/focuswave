namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSession.Start;

public record StartSessionRequest(Guid UserId, DateTimeOffset StartTime);
