using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.ForceEnd;

public record AcknowledgeEndMismatchRequest(Guid UserId, DateTime EndTime);
