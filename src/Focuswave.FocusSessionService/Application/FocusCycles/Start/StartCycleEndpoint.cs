using FastEndpoints;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Focuswave.FocusSessionService.Application.FocusCycles.Start;

// Using a type alias for the return type improves readability and simplifies the endpoint definition.
using ReturnType = Results<NoContent, BadRequest<string>>;

/// <summary>
/// Defines the endpoint for initiating a new focus cycle.
/// This endpoint is responsible for creating a new focus cycle aggregate and persisting it.
/// </summary>
/// <param name="repo">The repository for focus cycle data access.</param>
/// <param name="ed">The event dispatcher for handling domain events.</param>
public class StartCycleEndpoint(IFocusCycleRepository repo, IEventDispatcher ed)
    : Endpoint<StartCycleRequest, ReturnType>
{
    /// <summary>
    /// Configures the endpoint's route, permissions, and grouping.
    /// </summary>
    public override void Configure()
    {
        // Maps the endpoint to the POST "/start-cycle" route.
        Post("/start-cycle");
        // This endpoint does not require authentication.
        AllowAnonymous();
        // Groups this endpoint under the "FocusCycle" feature group for better organization.
        Group<FocusCycleGroup>();
    }

    /// <summary>
    /// Handles the asynchronous execution of the start cycle request.
    /// </summary>
    /// <param name="req">The request object containing the UserId and StartTime.</param>
    /// <param name="ct">A cancellation token for the asynchronous operation.</param>
    /// <returns>A result indicating success (NoContent) or failure (ProblemHttpResult).</returns>
    public override async Task<ReturnType> ExecuteAsync(StartCycleRequest req, CancellationToken ct)
    {
        if (await repo.GetByUserIdAsync(req.UserId) != null)
            return TypedResults.BadRequest("Already created");

        // The aggregate creation encapsulates business logic and validation.
        var res = FocusCycleAggregate.Create(req.UserId, req.StartTime, ed);

        // Persist the new aggregate only if creation was successful.
        if (res.IsSucc)
            await repo.SaveAsync(res.ThrowIfFail());

        // Dispatch any domain events that were raised during the aggregate's creation.
        await ed.DispatchAsync(ct);

        // Pattern match on the result to provide an appropriate HTTP response.
        return res.Match<ReturnType>(
            Succ: _ => TypedResults.NoContent(), // On success, return a 204 NoContent response.
            Fail: err => TypedResults.BadRequest(err.Message) // On failure, return a problem details response with the error message.
        );
    }
}
