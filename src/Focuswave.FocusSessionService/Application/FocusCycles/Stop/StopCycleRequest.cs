using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Stop;

public record StopCycleRequest(Guid UserId, DateTimeOffset StopTime);
