using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Focuswave.FocusSessionService.Application.FocusCycles.FocusSessions.ForceEnd;

using ReturnType = Results<NoContent, BadRequest<string>>;

public class AcknowledgeEndMismatchEndpoint(IFocusCycleRepository repo, IEventDispatcher ed, ILogger<AcknowledgeEndMismatchEndpoint> logger)
    : Endpoint<AcknowledgeEndMismatchRequest, ReturnType>
{
    public override void Configure()
    {
        Post("/acknowledge-end-mismatch");
        AllowAnonymous();
        Group<FocusCycleGroup>();
    }

    public override async Task<ReturnType> ExecuteAsync(
        AcknowledgeEndMismatchRequest req,
        CancellationToken ct
    )
    {
        logger.LogInformation("Attempting to acknowledge end mismatch for user {UserId}", req.UserId);

        var maybe = await repo.GetByUserIdAsync(req.UserId);
        if(maybe.IsNone)
        {
            logger.LogWarning("No active focus cycle found for user {UserId} to acknowledge end mismatch.", req.UserId);
        }

        var result = maybe
            .ToFin("cycle not found")
            .Bind(c => c.AcknowledgeEndMismatch(req.UserId, req.EndTime, ed).Map(_ => c));

        if (result.IsSucc)
        {
            await repo.SaveAsync(result.ThrowIfFail());
            logger.LogInformation("Successfully acknowledged end mismatch for user {UserId}", req.UserId);
        }
        else
        {
            logger.LogError("Failed to acknowledge end mismatch for user {UserId}: {Error}", req.UserId, result.Match(s => "", e => e.Message));
        }

        await ed.DispatchAsync(ct);

        return result.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(),
            Fail: e => TypedResults.BadRequest(e.Message)
        );
    }
}
