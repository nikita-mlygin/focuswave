using FastEndpoints;
using Focuswave.SessionTrackingService.Models;
using Focuswave.SessionTrackingService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Focuswave.SessionTrackingService.Endpoints.Get;

public class FocusCycleWithSegmentsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public List<FocusCycleSegmentDto> Segments { get; set; } = [];
}

public class FocusCycleSegmentDto
{
    public Guid Id { get; set; }
    public int SegmentIndex { get; set; }
    public FocusCycleSegmentType Type { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public TimeSpan? PlannedDuration { get; set; }
    public TimeSpan? ActualDuration { get; set; }
}

public class GetUserCyclesRequest
{
    public Guid UserId { get; set; }
}

public class GetUserCyclesEndpoint : Endpoint<GetUserCyclesRequest, List<FocusCycleWithSegmentsDto>>
{
    private readonly SessionTrackingDbContext ctx;

    public GetUserCyclesEndpoint(SessionTrackingDbContext db)
    {
        ctx = db;
    }

    public override void Configure()
    {
        Get("/user-cycles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetUserCyclesRequest req, CancellationToken ct)
    {
        var cycles = await ctx.FocusCycles.Where(c => c.UserId == req.UserId).ToListAsync(ct);

        var cycleIds = cycles.Select(c => c.Id).ToList();

        var segments = await ctx
            .FocusCycleSegments.Where(s => cycleIds.Contains(s.CycleId))
            .OrderBy(s => s.StartedAt)
            .ToListAsync(ct);

        var groupedSegments = segments
            .GroupBy(s => s.CycleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = cycles
            .Select(c => new FocusCycleWithSegmentsDto
            {
                Id = c.Id,
                UserId = c.UserId,
                StartedAt = c.StartedAt,
                EndedAt = c.EndedAt,
                Segments = groupedSegments.TryGetValue(c.Id, out var segs)
                    ?
                    [
                        .. segs.Select(s => new FocusCycleSegmentDto
                        {
                            Id = s.Id,
                            Type = s.Type,
                            StartedAt = s.StartedAt,
                            EndedAt = s.EndedAt,
                            SegmentIndex = s.Index,
                            PlannedDuration = s.PlannedDuration,
                            ActualDuration = s.ActualDuration,
                        }),
                    ]
                    : [],
            })
            .ToList();

        await Send.OkAsync(result, ct);
    }
}
