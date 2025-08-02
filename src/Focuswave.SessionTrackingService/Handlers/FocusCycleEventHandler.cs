using Focuswave.Integration.Events;
using Focuswave.SessionTrackingService.Models;
using Focuswave.SessionTrackingService.Persistence;
using Google.Protobuf;
using LanguageExt.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace Focuswave.SessionTrackingService.Handlers;

public class FocusCycleEventHandler(
    SessionTrackingDbContext ctx,
    ILogger<FocusCycleEventHandler> logger
)
{
    private readonly SessionTrackingDbContext ctx = ctx;
    private readonly ILogger<FocusCycleEventHandler> logger = logger;

    public Task HandleAsync(FocusCycleEvent @event, CancellationToken cancellationToken = default)
    {
        var cycleId = Guid.Parse(@event.FocusCycleId);
        var time = @event.EventTime.ToDateTimeOffset();

#if DEBUG
        logger.LogInformation("Event consumed: {@event}", @event);
#endif

        return @event.EventCase switch
        {
            FocusCycleEvent.EventOneofCase.StartWithoutDuration => HandleStartWithoutDurationAsync(
                cycleId,
                time,
                GetDetails(@event),
                cancellationToken
            ),
            FocusCycleEvent.EventOneofCase.StartWithDuration =>
                HandleStartFragmentWithDurationAsync(
                    cycleId,
                    time,
                    @event.StartWithDuration.PlannedDuration.ToTimeSpan(),
                    GetDetails(@event),
                    cancellationToken
                ),
            FocusCycleEvent.EventOneofCase.Ended => HandleEndEventAsync(
                cycleId,
                time,
                GetDetails(@event),
                cancellationToken
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(@event),
                "Event type is out of range"
            ),
        };
    }

    private static IMessage GetDetails(FocusCycleEvent @event) =>
        @event.DetailCase switch
        {
            FocusCycleEvent.DetailOneofCase.FocusSession => @event.FocusSession,
            FocusCycleEvent.DetailOneofCase.PlannedBreak => @event.PlannedBreak,
            FocusCycleEvent.DetailOneofCase.UnplannedInterruption => @event.UnplannedInterruption,
            FocusCycleEvent.DetailOneofCase.FocusCycle => @event.FocusCycle,
            FocusCycleEvent.DetailOneofCase.FocusSessionTooShort => @event.FocusSessionTooShort,
            _ => throw new ArgumentOutOfRangeException(
                nameof(@event),
                "Event type is out of range"
            ),
        };

    private Task HandleStartWithoutDurationAsync(
        Guid cycleId,
        DateTimeOffset startTime,
        IMessage details,
        CancellationToken cancellationToken = default
    ) =>
        details switch
        {
            FocusCycleDetail focusCycle => HandleStartFocusCycleAsync(
                cycleId,
                startTime,
                Guid.Parse(focusCycle.UserId),
                cancellationToken
            ),
            UnplannedInterruptionDetail unplannedInterruption =>
                HandleStartUnplannedInterruptionAsync(
                    cycleId,
                    startTime,
                    unplannedInterruption.SegmentIndex,
                    cancellationToken
                ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(@details),
                "Details type is out of range"
            ),
        };

    private Task HandleEndEventAsync(
        Guid cycleId,
        DateTimeOffset endTime,
        IMessage details,
        CancellationToken cancellationToken = default
    ) =>
        details switch
        {
            FocusCycleDetail focusCycleDetail => HandleEndFocusCycleAsync(
                cycleId,
                endTime,
                cancellationToken
            ),
            FocusSessionDetail focusSession => HandleEndFocusSessionAsync(
                cycleId,
                endTime,
                focusSession.SegmentIndex,
                cancellationToken
            ),
            PlannedBreakDetail plannedBreakDetail => HandleEndPlannedBreakAsync(
                cycleId,
                endTime,
                plannedBreakDetail.SegmentIndex,
                cancellationToken
            ),
            FocusSessionTooShortDetail focusSessionTooShort => HandleEndFocusSessionTooShortAsync(
                cycleId,
                endTime,
                focusSessionTooShort.SegmentIndex,
                cancellationToken
            ),
            UnplannedInterruptionDetail unplannedInterruption =>
                HandleEndUnplannedInterruptionAsync(
                    cycleId,
                    endTime,
                    unplannedInterruption.SegmentIndex,
                    cancellationToken
                ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(@details),
                "Details type is out of range"
            ),
        };

    private Task HandleStartFragmentWithDurationAsync(
        Guid id,
        DateTimeOffset startTime,
        TimeSpan duration,
        IMessage details,
        CancellationToken cancellationToken = default
    ) =>
        details switch
        {
            FocusSessionDetail focusSession => HandleStartFocusSessionAsync(
                id,
                startTime,
                focusSession.SegmentIndex,
                duration,
                cancellationToken
            ),
            PlannedBreakDetail plannedBreak => HandleStartPlannedBreakAsync(
                id,
                startTime,
                plannedBreak.SegmentIndex,
                duration,
                cancellationToken
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(@details),
                "Details type is out of range"
            ),
        };

    private async Task HandleStartFocusCycleAsync(
        Guid id,
        DateTimeOffset startTime,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var cycle = new FocusCycle
        {
            Id = id,
            StartedAt = startTime,
            UserId = userId,
            EndedAt = null,
        };

        await ctx.FocusCycles.AddAsync(cycle, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleStartFocusSessionAsync(
        Guid id,
        DateTimeOffset startTime,
        int index,
        TimeSpan duration,
        CancellationToken ct
    )
    {
        var segment = new FocusCycleSegment
        {
            Id = Guid.NewGuid(),
            CycleId = id,
            StartedAt = startTime,
            Index = index,
            Type = FocusCycleSegmentType.FocusSession,
            PlannedDuration = duration,
        };

        await ctx.FocusCycleSegments.AddAsync(segment, ct);
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleStartPlannedBreakAsync(
        Guid id,
        DateTimeOffset startTime,
        int index,
        TimeSpan duration,
        CancellationToken ct
    )
    {
        var segment = new FocusCycleSegment
        {
            Id = Guid.NewGuid(),
            CycleId = id,
            StartedAt = startTime,
            Index = index,
            Type = FocusCycleSegmentType.PlannedBreak,
            PlannedDuration = duration,
        };

        await ctx.FocusCycleSegments.AddAsync(segment, ct);
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleStartUnplannedInterruptionAsync(
        Guid id,
        DateTimeOffset startTime,
        int index,
        CancellationToken ct
    )
    {
        var segment = new FocusCycleSegment
        {
            Id = Guid.NewGuid(),
            CycleId = id,
            StartedAt = startTime,
            Index = index,
            Type = FocusCycleSegmentType.Interruption,
        };

        await ctx.FocusCycleSegments.AddAsync(segment, ct);
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleEndFocusCycleAsync(
        Guid id,
        DateTimeOffset endTime,
        CancellationToken ct
    )
    {
        var cycle = await ctx.FocusCycles.FindAsync([id], ct);
        if (cycle is null)
        {
            logger.LogWarning("FocusCycle not found for id {Id}", id);
            return;
        }

        cycle.EndedAt = endTime;
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleEndFocusSessionAsync(
        Guid id,
        DateTimeOffset endTime,
        int index,
        CancellationToken ct
    )
    {
        var segment = await ctx
            .FocusCycleSegments.Where(x =>
                x.CycleId == id && x.Index == index && x.Type == FocusCycleSegmentType.FocusSession
            )
            .FirstOrDefaultAsync(ct);

        if (segment is null)
        {
            logger.LogWarning(
                "FocusSession segment not found. CycleId={Id}, Index={Index}",
                id,
                index
            );
            return;
        }

        segment.EndedAt = endTime;
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleEndFocusSessionTooShortAsync(
        Guid id,
        DateTimeOffset endTime,
        int index,
        CancellationToken ct
    )
    {
        var segment = await ctx
            .FocusCycleSegments.Where(x =>
                x.CycleId == id && x.Index == index && x.Type == FocusCycleSegmentType.FocusSession
            )
            .FirstOrDefaultAsync(ct);

        if (segment is null)
        {
            logger.LogWarning(
                "FocusSession segment not found. CycleId={Id}, Index={Index}",
                id,
                index
            );
            return;
        }

        segment.EndedAt = endTime;
        segment.Type = FocusCycleSegmentType.EarlyEndedFocusSession;
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleEndPlannedBreakAsync(
        Guid id,
        DateTimeOffset endTime,
        int index,
        CancellationToken ct
    )
    {
        var segment = await ctx
            .FocusCycleSegments.Where(x =>
                x.CycleId == id && x.Index == index && x.Type == FocusCycleSegmentType.PlannedBreak
            )
            .FirstOrDefaultAsync(ct);

        if (segment is null)
        {
            logger.LogWarning(
                "PlannedBreak segment not found. CycleId={Id}, Index={Index}",
                id,
                index
            );
            return;
        }

        segment.EndedAt = endTime;
        await ctx.SaveChangesAsync(ct);
    }

    private async Task HandleEndUnplannedInterruptionAsync(
        Guid id,
        DateTimeOffset endTime,
        int index,
        CancellationToken ct
    )
    {
        var segment = await ctx
            .FocusCycleSegments.Where(x =>
                x.CycleId == id && x.Index == index && x.Type == FocusCycleSegmentType.Interruption
            )
            .FirstOrDefaultAsync(ct);

        if (segment is null)
        {
            logger.LogWarning(
                "UnplannedInterruption segment not found. CycleId={Id}, Index={Index}",
                id,
                index
            );
            return;
        }

        segment.EndedAt = endTime;
        await ctx.SaveChangesAsync(ct);
    }
}
