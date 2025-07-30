using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.Start;

// request record
public record StartPlannedBreakRequest(Guid UserId, DateTimeOffset StartedAt, TimeSpan Duration);
