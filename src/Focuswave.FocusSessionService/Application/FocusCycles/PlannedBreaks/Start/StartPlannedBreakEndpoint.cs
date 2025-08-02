using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging; // Add logging namespace

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.Start;

using ReturnType = Results<NoContent, BadRequest<string>>;

// endpoint
// endpoint
public class StartPlannedBreakEndpoint(
    IFocusCycleRepository repo,
    IEventDispatcher ed,
    ILogger<StartPlannedBreakEndpoint> logger
) // Inject ILogger
    : Endpoint<StartPlannedBreakRequest, ReturnType>
{
    public override void Configure()
    {
        Post("/start-planned-break");
        AllowAnonymous();
        Group<FocusCycleGroup>();
    }

    public override async Task<ReturnType> ExecuteAsync(
        StartPlannedBreakRequest req,
        CancellationToken ct
    )
    {
        logger.LogInformation("Attempting to start planned break for user {UserId}", req.UserId);

        var maybe = await repo.GetByUserIdAsync(req.UserId);
        if (maybe.IsNone)
        {
            logger.LogWarning(
                "No active focus cycle found for user {UserId} to start a break.",
                req.UserId
            );
        }

        var result = maybe
            .ToFin("Cycle not found")
            .Bind(c => c.StartBreak(req.UserId, req.StartedAt, req.Duration, ed).Map(_ => c));

        if (result.IsSucc)
        {
            await repo.SaveAsync(result.ThrowIfFail());
            logger.LogInformation(
                "Successfully started planned break for user {UserId}",
                req.UserId
            );
        }
        else
        {
            logger.LogError(
                "Failed to start planned break for user {UserId}: {Error}",
                req.UserId,
                result.Match(s => "", e => e.Message)
            );
        }

        await ed.DispatchAsync(ct);

        return result.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(),
            Fail: e => TypedResults.BadRequest(e.Message)
        );
    }
}
