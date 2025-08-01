using Focuswave.FocusSessionService.Application.IntegrationEvents;
using Focuswave.Integration;
using Focuswave.Integration.Events;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Focuswave.FocusSessionService.Application.FocusCycles;

public static class FocusCycleEventFactory
{
    public static FocusCycleEvent CreateStartEventWithDuration<TDetail>(
        Guid cycleId,
        DateTimeOffset eventTime,
        TimeSpan plannedDuration,
        TDetail detail
    )
        where TDetail : class, IMessage
    {
        var ev = new FocusCycleEvent
        {
            FocusCycleId = cycleId.ToString(),
            EventTime = Timestamp.FromDateTime(eventTime.UtcDateTime),
            StartWithDuration = new()
            {
                PlannedDuration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(
                    plannedDuration
                ),
            },
        };

        switch (detail)
        {
            case FocusSessionDetail dt:
                ev.FocusSession = dt;
                break;
            case PlannedBreakDetail dt:
                ev.PlannedBreak = dt;
                break;
            case FocusSessionTooShortDetail dt:
                ev.FocusSessionTooShort = dt;
                break;
            default:
                throw new ArgumentException("Invalid detail type");
        }

        return ev;
    }

    public static FocusCycleEvent CreateStartEventWithoutDuration<TDetail>(
        Guid cycleId,
        DateTimeOffset eventTime,
        TDetail detail
    )
        where TDetail : IMessage
    {
        var ev = new FocusCycleEvent
        {
            FocusCycleId = cycleId.ToString(),
            EventTime = Timestamp.FromDateTime(eventTime.UtcDateTime),
            StartWithoutDuration = new() { },
        };

        switch (detail)
        {
            case FocusCycleDetail dt:
                ev.FocusCycle = dt;
                break;
            case UnplannedInterruptionDetail dt:
                ev.UnplannedInterruption = dt;
                break;
            default:
                throw new ArgumentException("Invalid detail type");
        }

        return ev;
    }

    public static FocusCycleEvent CreateEndEvent<TDetail>(
        Guid cycleId,
        DateTimeOffset eventTime,
        TDetail detail
    )
        where TDetail : IMessage
    {
        var ev = new FocusCycleEvent
        {
            FocusCycleId = cycleId.ToString(),
            EventTime = Timestamp.FromDateTime(eventTime.UtcDateTime),
            Ended = new() { },
        };

        switch (detail)
        {
            case FocusSessionDetail dt:
                ev.FocusSession = dt;
                break;
            case PlannedBreakDetail dt:
                ev.PlannedBreak = dt;
                break;
            case UnplannedInterruptionDetail dt:
                ev.UnplannedInterruption = dt;
                break;
            case FocusSessionTooShortDetail dt:
                ev.FocusSessionTooShort = dt;
                break;
            case FocusCycleDetail dt:
                ev.FocusCycle = dt;
                break;
            default:
                throw new ArgumentException("Invalid detail type");
        }

        return ev;
    }

    public static TDetail GenerateDetail<TDetail>(int index)
        where TDetail : class, IMessage
    {
        object detail;

        if (typeof(TDetail) == typeof(FocusSessionDetail))
            detail = new FocusSessionDetail { SegmentIndex = index };
        else if (typeof(TDetail) == typeof(PlannedBreakDetail))
            detail = new PlannedBreakDetail { SegmentIndex = index };
        else if (typeof(TDetail) == typeof(UnplannedInterruptionDetail))
            detail = new UnplannedInterruptionDetail { SegmentIndex = index };
        else if (typeof(TDetail) == typeof(FocusSessionTooShortDetail))
            detail = new FocusSessionTooShortDetail { SegmentIndex = index };
        else
            throw new ArgumentException($"Unsupported detail type: {typeof(TDetail).Name}");

        return detail as TDetail
            ?? throw new InvalidCastException(
                $"Cannot cast {detail.GetType().Name} to {typeof(TDetail).Name}"
            );
    }

    public static FocusCycleDetail GenerateDetail(Guid userId)
    {
        return new FocusCycleDetail { UserId = userId.ToString() };
    }
}
