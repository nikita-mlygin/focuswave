namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.End;

public record EndSessionRequest(Guid UserId, DateTimeOffset EndTime);
