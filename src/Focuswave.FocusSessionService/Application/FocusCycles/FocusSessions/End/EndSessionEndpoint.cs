using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.End;

using ReturnType = Results<NoContent, BadRequest<string>>;

public class EndSessionEndpoint(IFocusCycleRepository repo, IEventDispatcher ed)
    : Endpoint<EndSessionRequest, ReturnType>
{
    public override void Configure()
    {
        Post("/end-session");
        AllowAnonymous();
        Group<FocusCycleGroup>();
    }

    public override async Task<ReturnType> ExecuteAsync(EndSessionRequest req, CancellationToken ct)
    {
        var maybe = await repo.GetByUserIdAsync(req.UserId);
        var result = maybe
            .ToFin("cycle not found")
            .Bind(c => c.EndSession(req.UserId, req.EndTime, ed).Map(_ => c));

        if (result.IsSucc)
            await repo.SaveAsync(result.ThrowIfFail());

        await ed.DispatchAsync(ct);

        return result.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(),
            Fail: e => TypedResults.BadRequest(e.Message)
        );
    }
}
