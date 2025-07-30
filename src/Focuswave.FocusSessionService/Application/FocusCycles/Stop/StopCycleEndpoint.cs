using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Focuswave.Integration;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Stop;

using ReturnType = Results<NoContent, BadRequest<string>, NotFound>;

public class StopCycleEndpoint(IFocusCycleRepository repo, IEventDispatcher ed)
    : Endpoint<StopCycleRequest, ReturnType>
{
    private readonly IFocusCycleRepository repo = repo;
    private readonly IEventDispatcher ed = ed;

    public override void Configure()
    {
        Post("stop-cycle");
        Group<FocusCycleGroup>();
        AllowAnonymous(); // TODO: Remove this once we have a way to authenticate the user.
    }

    public override async Task<ReturnType> ExecuteAsync(StopCycleRequest rq, CancellationToken ct)
    {
        var cycle = await repo.GetByUserIdAsync(rq.UserId);

        if (cycle.IsNone)
            return TypedResults.NotFound();

        var res = cycle
            .ToFin()
            .Bind(cycle => cycle.EndCycle(rq.UserId, rq.StopTime, ed).Map(_ => cycle));

        return await res.Match<Task<ReturnType>>(
            Succ: async cycle =>
            {
                await repo.SaveAsync(cycle);

                await ed.DispatchAsync();

                return TypedResults.NoContent();
            },
            Fail: err => Task.FromResult<ReturnType>(TypedResults.BadRequest(err.Message))
        );
    }
}
