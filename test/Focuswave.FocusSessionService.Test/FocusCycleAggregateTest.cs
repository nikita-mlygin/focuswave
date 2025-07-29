using FluentAssertions;
using Focuswave.Common.DomainEvents;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using Focuswave.FocusSessionService.Domain.FocusCycles.Events;
using NSubstitute;

namespace Focuswave.FocusSessionService.Test;

public class FocusCycleAggregateTests
{
    private readonly Guid userId = Guid.NewGuid();
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    private readonly IEventDispatcher ed = Substitute.For<IEventDispatcher>();

    private FocusCycleAggregate GenerateCycle(IEventDispatcher? ed = null) =>
        FocusCycleAggregate
            .Create(userId, now, ed ?? this.ed)
            .IfFail(_ =>
            {
                Assert.Fail();
                throw new Exception();
            });

    #region Create

    [Fact]
    public void Create_ShouldInitializeCycleCorrectly()
    {
        var ed = Substitute.For<IEventDispatcher>();
        var cycleResult = FocusCycleAggregate.Create(userId, now, ed);

        cycleResult.IsSucc.Should().BeTrue();
        var cycle = cycleResult.IfFail(_ =>
        {
            Assert.Fail();
            throw new Exception();
        });

        cycle.UserId.Should().Be(userId);
        cycle.StartedAt.IsSome.Should().BeTrue();
        cycle.FocusSession.IsNone.Should().BeTrue();
        cycle.PlannedBreaks.IsNone.Should().BeTrue();
        cycle.UnplannedInterruptions.IsNone.Should().BeTrue();

        ed.Received()
            .Publish(
                Arg.Is<FocusCycleStarted>(e =>
                    e.FocusCycleId == cycle.Id && e.UserId == userId && e.OccurredOn == now
                )
            );
    }

    #endregion

    #region EndCycle

    [Fact]
    public void EndCycle_ShouldPublishFocusSessionEnded_WhenSessionIsActive()
    {
        var ed = Substitute.For<IEventDispatcher>();
        var cycle = GenerateCycle(ed);

        // запускаем сессию
        var startSessionResult = cycle.StartSession(userId, now, ed);
        startSessionResult.IsSucc.Should().BeTrue();

        var result = cycle.EndCycle(userId, now.AddMinutes(40), ed);

        result.IsSucc.Should().BeTrue();
        cycle.StartedAt.IsNone.Should().BeTrue();

        ed.Received()
            .Publish(
                Arg.Is<FocusSessionEnded>(e =>
                    e.FocusCycleId == cycle.Id && e.OccurredOn == now.AddMinutes(40)
                )
            );
    }

    [Fact]
    public void EndCycle_ShouldPublishPlannedBreakEnded_WhenPlannedBreakIsActive()
    {
        var ed = Substitute.For<IEventDispatcher>();
        var cycle = GenerateCycle(ed);

        // запускаем планируемый перерыв
        var startBreakResult = cycle.StartBreak(userId, now, TimeSpan.FromMinutes(5), ed);
        startBreakResult.IsSucc.Should().BeTrue();

        var result = cycle.EndCycle(userId, now.AddMinutes(40), ed);

        result.IsSucc.Should().BeTrue();
        cycle.StartedAt.IsNone.Should().BeTrue();

        ed.Received()
            .Publish(
                Arg.Is<PlannedBreakEnded>(e =>
                    e.FocusCycleId == cycle.Id && e.OccurredOn == now.AddMinutes(40)
                )
            );
    }

    // TODO [Fact]
    private void EndCycle_ShouldPublishUnplannedInterruptionEnded_WhenUnplannedInterruptIsActive()
    {
        var ed = Substitute.For<IEventDispatcher>();
        var cycle = GenerateCycle(ed);

        // TODO добавить interrupt-ы

        var result = cycle.EndCycle(userId, now.AddMinutes(40), ed);

        result.IsSucc.Should().BeTrue();
        cycle.StartedAt.IsNone.Should().BeTrue();

        ed.Received()
            .Publish(
                Arg.Is<UnplannedInterruptionEnded>(e =>
                    e.FocusCycleId == cycle.Id && e.OccurredOn == now.AddMinutes(40)
                )
            );
    }

    [Fact]
    public void EndCycle_ShouldFail_WhenUserIdIsWrong()
    {
        var ed = Substitute.For<IEventDispatcher>();
        var cycle = GenerateCycle(ed);

        var result = cycle.EndCycle(Guid.NewGuid(), now.AddMinutes(40), ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<FocusCycleStopped>());
    }

    #endregion

    #region StartSession

    [Fact]
    public void StartSession_ShouldSucceed_WhenNoSessionRunning()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var result = cycle.StartSession(userId, now, ed);

        result.IsSucc.Should().BeTrue();
        cycle.FocusSession.IsSome.Should().BeTrue();
        cycle.FocusSession.IfSome(_ => _.StartedAt.Should().Be(now));
        ed.Received(1).Publish(Arg.Any<FocusSessionStarted>());
    }

    [Fact]
    public void StartSession_ShouldFail_WhenSessionAlreadyRunning()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        cycle.StartSession(userId, now, ed);
        var result = cycle.StartSession(userId, now.AddMinutes(1), ed);

        result.IsFail.Should().BeTrue();
        ed.Received(1).Publish(Arg.Any<FocusSessionStarted>());
    }

    [Fact]
    public void StartSession_ShouldFail_WhenWrongUser()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var result = cycle.StartSession(Guid.NewGuid(), now, ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<FocusSessionStarted>());
    }

    #endregion

    #region EndSession

    [Fact]
    public void EndSession_ShouldSucceed_WhenSessionIsRunning_AndDurationEnded()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var start = now;
        var end = start.AddMinutes(31);

        cycle.StartSession(userId, start, ed);
        var result = cycle.EndSession(userId, end, ed);

        result.IsSucc.Should().BeTrue();
        cycle.FocusSession.IsNone.Should().BeTrue();
        ed.Received()
            .Publish(
                Arg.Is<FocusSessionEnded>(e => e.FocusCycleId == cycle.Id && e.OccurredOn == end)
            );
    }

    [Fact]
    public void EndSession_ShouldFail_WhenWrongUser()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        cycle.StartSession(userId, now, ed);
        var result = cycle.EndSession(Guid.NewGuid(), now.AddMinutes(31), ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<FocusSessionEnded>());
    }

    [Fact]
    public void EndSession_ShouldFail_WhenSessionIsNotRunning()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var result = cycle.EndSession(userId, now.AddMinutes(30), ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<FocusSessionEnded>());
    }

    [Fact]
    public void EndSession_ShouldFail_WhenDurationNotEnded()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var start = now;
        var earlyEnd = start.AddMinutes(10);

        cycle.StartSession(userId, start, ed);
        var result = cycle.EndSession(userId, earlyEnd, ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<FocusSessionEnded>());
    }
    #endregion

    #region StartBreak

    [Fact]
    public void StartBreak_ShouldPublishPlannedBreakStarted_WhenSessionNotActive()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var result = cycle.StartBreak(userId, now, TimeSpan.FromMinutes(5), ed);

        result.IsSucc.Should().BeTrue();
        ed.Received()
            .Publish(
                Arg.Is<PlannedBreakStarted>(e => e.FocusCycleId == cycle.Id && e.OccurredOn == now)
            );
    }

    [Fact]
    public void StartBreak_ShouldFail_WhenBreakAlreadyActive()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        cycle.StartBreak(userId, now, TimeSpan.FromMinutes(5), ed);
        var result = cycle.StartBreak(userId, now.AddMinutes(1), TimeSpan.FromMinutes(5), ed);

        result.IsFail.Should().BeTrue();
        ed.Received(1).Publish(Arg.Any<PlannedBreakStarted>());
    }

    [Fact]
    public void StartBreak_ShouldInterruptSession_WhenSessionActive()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        cycle.StartSession(userId, now, ed);
        var result = cycle.StartBreak(userId, now.AddMinutes(10), TimeSpan.FromMinutes(5), ed);

        result.IsSucc.Should().BeTrue();
        ed.Received().Publish(Arg.Any<FocusSessionEnded>());
        ed.Received().Publish(Arg.Any<PlannedBreakStarted>());
    }

    [Fact]
    public void StartBreak_ShouldFail_WhenWrongUser()
    {
        var cycle = GenerateCycle();
        var ed = Substitute.For<IEventDispatcher>();

        var result = cycle.StartBreak(Guid.NewGuid(), now, TimeSpan.FromMinutes(5), ed);

        result.IsFail.Should().BeTrue();
        ed.DidNotReceive().Publish(Arg.Any<PlannedBreakStarted>());
    }

    #endregion

    #region GetCycleState

    [Fact]
    public void GetCycleState_ShouldReturnCorrectState()
    {
        var cycle = GenerateCycle();

        var ed = Substitute.For<IEventDispatcher>();
        cycle.StartSession(userId, now, ed);
        cycle.GetCycleState().Should().Be(CycleState.SessionActive);

        cycle.EndSession(userId, now.AddMinutes(10), ed);
        cycle.StartBreak(userId, now.AddMinutes(11), TimeSpan.FromMinutes(5), ed);
        cycle.GetCycleState().Should().Be(CycleState.PlannedBreak);
    }

    #endregion
}
