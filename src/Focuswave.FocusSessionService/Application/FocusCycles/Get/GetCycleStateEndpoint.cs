using FastEndpoints;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Get;

using ReturnType = Results<Ok<FocusSessionStateDto>, NotFound>;

public record GetSessionStateRequest([FromRoute] Guid UserId);

public class GetSessionStateEndpoint(IFocusCycleRepository repo)
    : Endpoint<GetSessionStateRequest, ReturnType>
{
    public override void Configure()
    {
        Get("/session-state/{UserId}");
        AllowAnonymous();
        Group<FocusCycleGroup>();
    }

    [Microsoft.AspNetCore.Mvc.HttpGet("/session-state/{UserId}")]
    public override async Task<ReturnType> ExecuteAsync(
        [FromRoute] GetSessionStateRequest req,
        CancellationToken ct
    )
    {
        var maybeCycle = await repo.GetByUserIdAsync(req.UserId);

        return maybeCycle.Match<ReturnType>(
            Some: cycle =>
            {
                var dto = new FocusSessionStateDto(
                    FocusCycleId: cycle.Id,
                    Status: cycle.GetCycleState(),
                    StartedAt: cycle.StartedAt.IfNoneUnsafe(null!),
                    FocusSession: cycle.FocusSession.IfNoneUnsafe((FocusSession?)null),
                    PlannedBreak: cycle.PlannedBreaks.IfNoneUnsafe((PlannedBreak?)null),
                    UnplannedInterruption: cycle.UnplannedInterruptions.IfNoneUnsafe(
                        (UnplannedInterruption?)null
                    )
                );

                return TypedResults.Ok(dto);
            },
            None: () => TypedResults.NotFound()
        );
    }
}
