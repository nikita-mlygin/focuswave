using Focuswave.Common.Domain;

namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public static class FocusCycleErrors
{
    public static readonly DomainError UserHaveNotPermission = new(1, "UserHaveNotPermission");
    public static readonly DomainError AlreadySessionStarted = new(2, "Session already started");
    public static readonly DomainError AlreadyBreakStarted = new(3, "Already break started");
    public static readonly DomainError SessionAlreadyStopped = new(4, "Session already stopped");
    public static readonly DomainError SessionCantBeLessThenDuration = new(
        5,
        "Session can't be less then duration"
    );

    public static readonly DomainError UserIdCannotBeEmpty = new(6, "User id cannot be empty");
    public static readonly DomainError SessionDurationExceededPlanned = new(
        7,
        "Actual session duration exceeds the planned duration"
    );
    public static readonly DomainError BreakAlreadyStopped = new(8, "Break already stopped");
}
