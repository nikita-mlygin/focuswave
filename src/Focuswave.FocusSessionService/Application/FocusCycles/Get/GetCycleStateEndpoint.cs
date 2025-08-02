using FastEndpoints;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Get;

using ReturnType = Results<Ok<FocusSessionStateDto>, NotFound>;

public record GetSessionStateRequest([FromRoute] Guid UserId);

public class GetSessionStateEndpoint(
    IFocusCycleRepository repo,
    ILogger<GetSessionStateEndpoint> logger
) : Endpoint<GetSessionStateRequest, ReturnType>
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
        logger.LogInformation("Attempting to get session state for user {UserId}", req.UserId);
        var maybeCycle = await repo.GetByUserIdAsync(req.UserId);

        return maybeCycle.Match<ReturnType>(
            Some: cycle =>
            {
                var dto = new FocusSessionStateDto(
                    FocusCycleId: cycle.Id,
                    Status: cycle.GetCycleState(),
                    StartedAt: cycle.StartedAt.MatchUnsafe<DateTimeOffset?>(
                        some => some,
                        () => null
                    ),
                    FocusSession: cycle.FocusSession.IfNoneUnsafe((FocusSession?)null),
                    PlannedBreak: cycle.PlannedBreaks.IfNoneUnsafe((PlannedBreak?)null),
                    UnplannedInterruption: cycle.UnplannedInterruptions.IfNoneUnsafe(
                        (UnplannedInterruption?)null
                    )
                );

                logger.LogInformation(
                    "Successfully retrieved session state for user {UserId}",
                    req.UserId
                );

                return TypedResults.Ok(dto);
            },
            None: () =>
            {
                logger.LogWarning("No active focus cycle found for user {UserId}", req.UserId);
                return TypedResults.NotFound();
            }
        );
    }
}
