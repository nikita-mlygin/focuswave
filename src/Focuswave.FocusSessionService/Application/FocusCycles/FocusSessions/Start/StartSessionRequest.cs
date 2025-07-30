namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.Start;

public record StartSessionRequest(Guid UserId, DateTimeOffset StartTime);
