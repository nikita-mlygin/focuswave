using System.Runtime.CompilerServices;
using Focuswave;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using LanguageExt.SomeHelp;
using LanguageExt.UnsafeValueAccess;
using static LanguageExt.Prelude;

namespace Focuswave.FocusSessionService.Domain.FocusCycles;

public class FocusCycleAggregate
{
    #region Properties
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public Option<DateTimeOffset> StartedAt { get; private set; }

    public Option<FocusSession> FocusSession { get; private set; }
    public Option<PlannedBreak> PlannedBreaks { get; private set; }

    public Option<UnplannedInterruption> UnplannedInterruptions { get; private set; }
    #endregion

    #region Construct
    private FocusCycleAggregate(Guid id, Guid userId, DateTimeOffset startedAt)
    {
        Id = id;
        UserId = userId;
        StartedAt = Some(startedAt);
        FocusSession = Option<FocusSession>.None;
        PlannedBreaks = Option<PlannedBreak>.None;
        UnplannedInterruptions = Option<UnplannedInterruption>.None;
    }

    private FocusCycleAggregate(
        Guid id,
        Guid userId,
        DateTimeOffset? startedAt,
        FocusSession? focusSession,
        PlannedBreak? plannedBreak,
        UnplannedInterruption? unplannedInterruption
    )
    {
        Id = id;
        UserId = userId;
        StartedAt = startedAt.ToOption();
        FocusSession = focusSession is not null ? focusSession : None;
        PlannedBreaks = plannedBreak is not null ? plannedBreak : None;
        UnplannedInterruptions = unplannedInterruption is not null ? unplannedInterruption : None;
    }

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
            .Do(_ => ed.Publish(new FocusSessionEnded(this.Id, endTime)));

        this.PlannedBreaks.Do(_ => this.PlannedBreaks = None)
            .Do(_ => ed.Publish(new PlannedBreakEnded(this.Id, endTime)));

        this.UnplannedInterruptions.Do(_ => this.UnplannedInterruptions = None)
            .Do(_ => ed.Publish(new UnplannedInterruptionEnded(this.Id, endTime)));

        this.StartedAt = None;

        ed.Publish(new FocusCycleStopped(this.Id, this.UserId, endTime));

        return Unit.Default;
    }

    public Fin<Unit> StartSession(
        Guid userId,
        DateTimeOffset sessionStartTime,
        IEventDispatcher ed
    ) =>
        CheckUser(userId)
            .Bind(_ => TestFailIfSessionStarted())
            .Bind(_ => EndBreakIfStarted(sessionStartTime, ed))
            .Do(_ =>
                this.FocusSession = new FocusSession(sessionStartTime, TimeSpan.FromMinutes(30))
            )
            .Do(_ =>
                ed.Publish(
                    new FocusSessionStarted(this.Id, sessionStartTime, TimeSpan.FromMinutes(30))
                )
            ); // TODO доделать дополнительный параметр

    public Fin<Unit> EndSession(Guid userId, DateTimeOffset sessionEndTime, IEventDispatcher ed) =>
        CheckUser(userId)
            .Bind(_ =>
                TestFailIfSessionNotStarted(
                    session =>
                        TestFailSessionIfNotDurationEnded(
                            session.StartedAt,
                            sessionEndTime,
                            session.Duration
                        ),
                    FocusCycleErrors.SessionCantBeLessThenDuration
                )
            )
            .Do(_ =>
                this.FocusSession.Do(_ => this.FocusSession = None)
                    .Do(_ => ed.Publish(new FocusSessionEnded(this.Id, sessionEndTime)))
            )
            .Do(_ => this.FocusSession = None)
            .Do(_ => ed.Publish(new FocusSessionEnded(this.Id, sessionEndTime)));

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
                    Right: __ =>
                        Fin<Unit>.Succ(
                            EndSession(ed, new FocusSessionEnded(this.Id, breakStartTime))
                        ),
                    Left: __ =>
                        this.PlannedBreaks.IsSome
                            ? Fin<Unit>.Fail(FocusCycleErrors.AlreadyBreakStarted)
                            : Fin<Unit>.Succ(Unit.Default)
                )
            )
            .Do(_ => this.PlannedBreaks = Some(new PlannedBreak(breakStartTime, duration)))
            .Do(_ => ed.Publish(new PlannedBreakStarted(this.Id, breakStartTime, duration)));

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

    private Fin<Unit> TestFailIfSessionNotStarted(
        Func<FocusSession, bool>? testSession = null,
        Error? error = null
    ) =>
        FocusSession.Match(
            _ =>
                testSession is not null && error is not null && testSession(_)
                    ? Fin<Unit>.Succ(Unit.Default)
                    : Fin<Unit>.Fail(error),
            Fin<Unit>.Fail(FocusCycleErrors.SessionAlreadyStopped)
        );

    private Unit EndSession(IEventDispatcher ed, FocusSessionEnded e)
    {
        this.FocusSession = None;
        ed.Publish(e);

        return Unit.Default;
    }

    private static bool TestFailSessionIfNotDurationEnded(
        DateTimeOffset start,
        DateTimeOffset now,
        TimeSpan duration
    ) => (now - start).Duration() > duration;

    private Fin<Unit> EndBreakIfStarted(DateTimeOffset endTime, IEventDispatcher ed)
    {
        this.PlannedBreaks.Do(_ => ed.Publish(new PlannedBreakEnded(this.Id, endTime)))
            .Do(_ => this.PlannedBreaks = None);

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
        UnplannedInterruption? UnplannedInterruption
    );

    public static FocusCycleAggregate Restore(Snapshot snapshot)
    {
        return new FocusCycleAggregate(
            snapshot.Id,
            snapshot.Userid,
            snapshot.StartedAt,
            snapshot.FocusSession,
            snapshot.PlannedBreak,
            snapshot.UnplannedInterruption
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
            this.UnplannedInterruptions.MatchUnsafe<UnplannedInterruption?>(x => x, () => null)
        );
    }
    #endregion
}
