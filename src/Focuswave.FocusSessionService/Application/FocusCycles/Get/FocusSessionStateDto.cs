using System.Text.Json.Serialization;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Get;

public record FocusSessionStateDto(
    Guid FocusCycleId,
    CycleState Status,
    DateTimeOffset? StartedAt,
    FocusSession? FocusSession,
    PlannedBreak? PlannedBreak,
    UnplannedInterruption? UnplannedInterruption
);
