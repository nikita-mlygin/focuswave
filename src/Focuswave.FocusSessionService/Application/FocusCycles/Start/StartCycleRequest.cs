using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

public record StartCycleRequest(Guid UserId, DateTimeOffset StartTime);
