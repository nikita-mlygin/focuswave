using FastEndpoints;
using Focuswave.Common.Domain;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSession.Start;

public class StartSessionEndpoint(IFocusCycleRepository repo, IEventDispatcher ed)
    : Endpoint<StartSessionRequest, Results<NoContent, BadRequest<string>, ProblemHttpResult>>
{
    private readonly IFocusCycleRepository repo = repo;
    private readonly IEventDispatcher ed = ed;

    public override void Configure()
    {
        Post("/start-session");
        AllowAnonymous(); // или настрой авторизацию
        Group<FocusCycleGroup>();
    }

    public override async Task<
        Results<NoContent, BadRequest<string>, ProblemHttpResult>
    > ExecuteAsync(StartSessionRequest req, CancellationToken ct)
    {
        var maybeCycle = await repo.GetByUserIdAsync(req.UserId);

        Fin<FocusCycleAggregate> res = maybeCycle
            .Match(
                Some: cycle => FinSucc(cycle),
                None: () => FocusCycleAggregate.Create(req.UserId, req.StartTime, ed)
            )
            .Bind(cycle => cycle.StartSession(req.UserId, req.StartTime, ed).Map(_ => cycle));

        if (res.IsSucc)
            await repo.SaveAsync(res.ThrowIfFail());

        await ed.DispatchAsync(ct);

        return res.Match<Results<NoContent, BadRequest<string>, ProblemHttpResult>>(
            Succ: _ => TypedResults.NoContent(),
            Fail: err =>
            {
                if (err is DomainError de)
                    return TypedResults.BadRequest(de.Message);
                return TypedResults.Problem(err.Message);
            }
        );
    }
}
