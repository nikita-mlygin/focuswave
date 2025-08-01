using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using LanguageExt.UnsafeValueAccess;

namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public class FocusCycleAggregate
{
    #region Properties
    /// <summary>
    /// Represents the unique identifier of the focus cycle.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Represents the unique identifier of the user associated with the focus cycle.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Represents the start time of the focus cycle, if it has started.
    /// </summary>
    public Option<DateTimeOffset> StartedAt { get; private set; }

    /// <summary>
    /// Represents the current focus session, if one is active.
    /// </summary>
    public Option<FocusSession> FocusSession { get; private set; }

    /// <summary>
    /// Represents the current planned break, if one is active.
    /// </summary>
    public Option<PlannedBreak> PlannedBreaks { get; private set; }

    /// <summary>
    /// Represents the current unplanned interruption, if one is active.
    /// </summary>
    public Option<UnplannedInterruption> UnplannedInterruptions { get; private set; }

    /// <summary>
    /// Represents the offset for the index of sessions/breaks within the focus cycle.
    /// </summary>
    public int IndexOffset { get; private set; }
    #endregion

    #region Construct
    /// <summary>
    /// Initializes a new instance of the <see cref="FocusCycleAggregate"/> class with a new unique identifier and start time.
    /// </summary>
    /// <param name="id">The unique identifier for the focus cycle.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="startedAt">The time at which the focus cycle started.</param>
    private FocusCycleAggregate(Guid id, Guid userId, DateTimeOffset startedAt)
    {
        Id = id;
        UserId = userId;
        StartedAt = Some(startedAt);
        FocusSession = Option<FocusSession>.None;
        PlannedBreaks = Option<PlannedBreak>.None;
        UnplannedInterruptions = Option<UnplannedInterruption>.None;
        IndexOffset = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusCycleAggregate"/> class from existing data, typically for restoring state.
    /// </summary>
    /// <param name="id">The unique identifier for the focus cycle.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="startedAt">The optional time at which the focus cycle started.</param>
    /// <param name="focusSession">The optional current focus session.</param>
    /// <param name="plannedBreak">The optional current planned break.</param>
    /// <param name="unplannedInterruption">The optional current unplanned interruption.</param>
    /// <param name="indexOffset">The offset for the index of sessions/breaks within the focus cycle.</param>
    private FocusCycleAggregate(
        Guid id,
        Guid userId,
        DateTimeOffset? startedAt,
        FocusSession? focusSession,
        PlannedBreak? plannedBreak,
        UnplannedInterruption? unplannedInterruption,
        int indexOffset
    )
    {
        Id = id;
        UserId = userId;
        StartedAt = startedAt.ToOption();
        FocusSession = focusSession is not null ? focusSession : None;
        PlannedBreaks = plannedBreak is not null ? plannedBreak : None;
        UnplannedInterruptions = unplannedInterruption is not null ? unplannedInterruption : None;
        IndexOffset = indexOffset;
    }

    /// <summary>
    /// Creates a new <see cref="FocusCycleAggregate"/> instance.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="startTime">The time at which the focus cycle starts.</param>
    /// <param name="ed">The event dispatcher to publish domain events.</param>
    /// <returns>A <see cref="Fin{A}"/> indicating success or failure, containing the new <see cref="FocusCycleAggregate"/> instance.</returns>
    public static Fin<FocusCycleAggregate> Create(
        Guid userId,
        DateTimeOffset startTime,
        IEventDispatcher ed
    )
    {
        if (userId == Guid.Empty)
            return FocusCycleErrors.UserIdCannotBeEmpty;

        var res = new FocusCycleAggregate(Guid.NewGuid(), userId, startTime);

        ed.Publish(new FocusCycleStarted(res.Id, res.UserId, res.StartedAt.Value()));

        return res;
    }
    #endregion

    #region Public Methods
    public CycleState GetCycleState() =>
        StartedAt
            .Match(_ => None, () => Some(CycleState.NotStarted))
            .Or(UnplannedInterruptions.Bind(_ => Some(CycleState.UnplannedBreak)))
            .Or(FocusSession.Bind(_ => Some(CycleState.SessionActive)))
            .IfNone(CycleState.PlannedBreak);

    public Fin<Unit> EndCycle(Guid userId, DateTimeOffset endTime, IEventDispatcher ed)
    {
        if (CheckUser(userId) is Fin<Unit> err && err.IsFail)
        {
            return err;
        }

        this.FocusSession.Do(_ => this.FocusSession = None)
            .Do(_ => ed.Publish(new FocusSessionEnded(this.Id, IndexOffset, endTime)));

        this.PlannedBreaks.Do(_ => this.PlannedBreaks = None)
            .Do(_ => ed.Publish(new PlannedBreakEnded(this.Id, IndexOffset, endTime)));

        this.UnplannedInterruptions.Do(_ => this.UnplannedInterruptions = None)
            .Do(_ => ed.Publish(new UnplannedInterruptionEnded(this.Id, IndexOffset, endTime)));

        this.StartedAt = None;

        ed.Publish(new FocusCycleStopped(this.Id, this.UserId, endTime));

        return Unit.Default;
    }

    public Fin<Unit> StartSession( // TODO add end cycle if break to long
        Guid userId,
        DateTimeOffset sessionStartTime,
        IEventDispatcher ed
    ) =>
        CheckUser(userId)
            .Bind(_ => TestFailIfSessionStarted())
            .Map(__ =>
            {
                _ = PlannedBreaks.Map(pb => EndBreak(pb, sessionStartTime, ed));
                return Unit.Default;
            })
            .Do(_ =>
                this.FocusSession = new FocusSession(sessionStartTime, TimeSpan.FromMinutes(30))
            )
            .Do(_ =>
                ed.Publish(
                    new FocusSessionStarted(
                        this.Id,
                        IndexOffset,
                        sessionStartTime,
                        TimeSpan.FromMinutes(30)
                    )
                )
            ); // TODO доделать дополнительный параметр

    public Fin<Unit> EndSession(Guid userId, DateTimeOffset sessionEndTime, IEventDispatcher ed) =>
        CheckUser(userId).Bind(_ => EndSession(ed, sessionEndTime));

    public Fin<Unit> StartBreak(
        Guid userId,
        DateTimeOffset breakStartTime,
        TimeSpan duration,
        IEventDispatcher ed
    ) =>
        CheckUser(userId)
            .Map(_ => FocusSession.ToEither(Unit.Default))
            .Bind(_ =>
                _.Match(
                    Right: fs => EndSession(fs, ed, breakStartTime),
                    Left: __ =>
                        this.PlannedBreaks.IsSome
                            ? Fin<Unit>.Fail(FocusCycleErrors.AlreadyBreakStarted)
                            : Fin<Unit>.Succ(Unit.Default)
                )
            )
            .Do(_ => this.PlannedBreaks = new PlannedBreak(breakStartTime, duration))
            .Do(_ =>
                ed.Publish(new PlannedBreakStarted(this.Id, IndexOffset, breakStartTime, duration))
            );

    public Fin<Unit> AcknowledgeEndMismatch(
        Guid userId,
        DateTimeOffset endTime,
        IEventDispatcher ed
    ) => CheckUser(userId).Bind(_ => AcknowledgeEndMismatch(endTime, ed));

    #endregion

    #region Utils
    private Fin<Unit> CheckUser(Guid userId)
    {
        return userId == this.UserId
            ? Fin<Unit>.Succ(Unit.Default)
            : Fin<Unit>.Fail(FocusCycleErrors.UserHaveNotPermission);
    }

    private Fin<Unit> TestFailIfSessionStarted() =>
        FocusSession.Match(
            _ => Fin<Unit>.Fail(FocusCycleErrors.AlreadySessionStarted),
            Fin<Unit>.Succ(Unit.Default)
        );

    private Fin<Unit> AcknowledgeEndMismatch(DateTimeOffset endTime, IEventDispatcher eq)
    {
        return this
            .FocusSession.ToFin(FocusCycleErrors.SessionAlreadyStopped)
            .Bind(session => AcknowledgeEndMismatch(session, eq, endTime));
    }

    private Fin<Unit> AcknowledgeEndMismatch(
        FocusSession session,
        IEventDispatcher ed,
        DateTimeOffset endTime
    )
    {
        if (TestFailSessionIfNotDurationEnded(session.StartedAt, endTime, session.Duration))
            return FocusCycleErrors.SessionDurationExceededPlanned;

        this.FocusSession = None;
        ed.Publish(new FocusSessionTooShortEvent(this.Id, IndexOffset, endTime));

        IndexOffset++;

        return Unit.Default;
    }

    // End session (set None) and increment indexOffset if session exists end duration is ok
    private Fin<Unit> EndSession(IEventDispatcher ed, DateTimeOffset endedTime)
    {
        return this
            .FocusSession.ToFin(FocusCycleErrors.SessionAlreadyStopped)
            .Bind(session => EndSession(session, ed, endedTime));
    }

    // End session (set None) and increment indexOffset if duration is ok
    private Fin<Unit> EndSession(
        FocusSession session,
        IEventDispatcher ed,
        DateTimeOffset endedTime
    )
    {
        if (!TestFailSessionIfNotDurationEnded(session.StartedAt, endedTime, session.Duration))
            return Fin<Unit>.Fail(FocusCycleErrors.SessionCantBeLessThenDuration);

        this.FocusSession = None;
        ed.Publish(new FocusSessionEnded(this.Id, this.IndexOffset, endedTime));
        this.IndexOffset++;

        return Unit.Default;
    }

    private static bool TestFailSessionIfNotDurationEnded(
        DateTimeOffset start,
        DateTimeOffset now,
        TimeSpan duration
    ) => now - start > duration;

    private Fin<Unit> EndBreak(DateTimeOffset endTime, IEventDispatcher ed)
    {
        return this
            .PlannedBreaks.ToFin(FocusCycleErrors.BreakAlreadyStopped)
            .Bind(pb => EndBreak(pb, endTime, ed));
    }

    private Fin<Unit> EndBreak(
        PlannedBreak plannedBreak,
        DateTimeOffset endTime,
        IEventDispatcher ed
    )
    {
        this.PlannedBreaks = None;
        ed.Publish(new PlannedBreakEnded(this.Id, IndexOffset, endTime));
        IndexOffset++;
        return Unit.Default;
    }
    #endregion

    #region Snapshot
    public record Snapshot(
        Guid Id,
        Guid Userid,
        DateTimeOffset? StartedAt,
        FocusSession? FocusSession,
        PlannedBreak? PlannedBreak,
        UnplannedInterruption? UnplannedInterruption,
        int IndexOffset
    );

    public static FocusCycleAggregate Restore(Snapshot snapshot)
    {
        return new FocusCycleAggregate(
            snapshot.Id,
            snapshot.Userid,
            snapshot.StartedAt,
            snapshot.FocusSession,
            snapshot.PlannedBreak,
            snapshot.UnplannedInterruption,
            snapshot.IndexOffset
        );
    }

    public Snapshot To()
    {
        return new Snapshot(
            this.Id,
            this.UserId,
            this.StartedAt.MatchUnsafe<DateTimeOffset?>(x => x, () => null),
            this.FocusSession.MatchUnsafe<FocusSession?>(x => x, () => null),
            this.PlannedBreaks.MatchUnsafe<PlannedBreak?>(x => x, () => null),
            this.UnplannedInterruptions.MatchUnsafe<UnplannedInterruption?>(x => x, () => null),
            this.IndexOffset
        );
    }
    #endregion
}
