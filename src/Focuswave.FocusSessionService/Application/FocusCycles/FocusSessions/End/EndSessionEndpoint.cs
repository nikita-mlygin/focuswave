using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.End;

using ReturnType = Results<NoContent, BadRequest<string>>;

public class EndSessionEndpoint(
    IFocusCycleRepository repo,
    IEventDispatcher ed,
    ILogger<EndSessionEndpoint> logger
) : Endpoint<EndSessionRequest, ReturnType>
{
    public override void Configure()
    {
        Post("/end-session");
        AllowAnonymous();
        Group<FocusCycleGroup>();
    }

    public override async Task<ReturnType> ExecuteAsync(EndSessionRequest req, CancellationToken ct)
    {
        logger.LogInformation("Attempting to end session for user {UserId}", req.UserId);
        var maybe = await repo.GetByUserIdAsync(req.UserId);
        var result = maybe
            .ToFin("cycle not found") // TODO 404
            .Bind(c => c.EndSession(req.UserId, req.EndTime, ed).Map(_ => c));

        if (result.IsSucc)
        {
            logger.LogInformation("Successfully ended session for user {UserId}", req.UserId);
            await repo.SaveAsync(result.ThrowIfFail());
        }

        await ed.DispatchAsync(ct);

        return result.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(),
            Fail: e => TypedResults.BadRequest(e.Message)
        );
    }
}
