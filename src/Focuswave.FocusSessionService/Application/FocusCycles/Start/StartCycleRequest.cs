using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

public record StartCycleRequest(Guid UserId, DateTimeOffset StartTime);
