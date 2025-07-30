using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.PlannedBreaks.Start;

using ReturnType = Results<NoContent, BadRequest<string>>;

// endpoint
public class StartPlannedBreakEndpoint(IFocusCycleRepository repo, IEventDispatcher ed)
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
        var maybe = await repo.GetByUserIdAsync(req.UserId);
        var result = maybe
            .ToFin("Cycle not found")
            .Bind(c => c.StartBreak(req.UserId, req.StartedAt, req.Duration, ed).Map(_ => c));

        if (result.IsSucc)
            await repo.SaveAsync(result.ThrowIfFail());

        await ed.DispatchAsync(ct);

        return result.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(),
            Fail: e => TypedResults.BadRequest(e.Message)
        );
    }
}
