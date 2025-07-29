namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public enum CycleState
{
    NotStarted,
    SessionActive,
    PlannedBreak,
    UnplannedBreak,
    IdleBetweenSessions,
}
