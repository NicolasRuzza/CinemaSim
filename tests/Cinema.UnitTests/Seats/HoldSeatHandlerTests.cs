using AwesomeAssertions;
using Cinema.Api.Features.Seats.HoldSeat;
using Microsoft.Extensions.Options;
using Moq;

namespace Cinema.UnitTests.Seats;

/// <summary>How the handler collaborates with its ports. The behavior seen from outside is covered by the BDD scenarios.</summary>
public sealed class HoldSeatHandlerTests
{
    // Different from the 5-minute default, so passing the wrong value cannot go unnoticed.
    private static readonly TimeSpan ConfiguredDuration = TimeSpan.FromSeconds(42);

    private readonly Mock<ISessionSchedule> _schedule = new();
    private readonly Mock<ISeatLockStore> _seatLocks = new();
    private readonly HoldSeatHandler _handler;

    public HoldSeatHandlerTests()
    {
        _handler = new HoldSeatHandler(
            _schedule.Object,
            _seatLocks.Object,
            Options.Create(new HoldSeatOptions { HoldDuration = ConfiguredDuration }));
    }

    [Fact]
    public async Task A_nonexistent_session_is_rejected_without_taking_any_lock()
    {
        _schedule.Setup(schedule => schedule.SessionExists("S9")).Returns(false);

        var result = await Handle("S9", "A1", "Bob");

        result.Should().Be(HoldSeatResult.Rejected(HoldRejectionReason.SessionNotFound));
        VerifyNoLockWasTaken();
    }

    [Fact]
    public async Task A_nonexistent_seat_is_rejected_without_taking_any_lock()
    {
        _schedule.Setup(schedule => schedule.SessionExists("S1")).Returns(true);
        _schedule.Setup(schedule => schedule.SeatExists("S1", "Z9")).Returns(false);

        var result = await Handle("S1", "Z9", "Bob");

        result.Should().Be(HoldSeatResult.Rejected(HoldRejectionReason.SeatNotFound));
        VerifyNoLockWasTaken();
    }

    [Fact]
    public async Task The_lock_is_requested_once_with_the_configured_duration_and_the_cancellation_token()
    {
        using var cancellation = new CancellationTokenSource();
        ExistingSeat("S1", "A1");
        LockOutcomeIs(SeatLockOutcome.Acquired);

        await _handler.Handle(new HoldSeatCommand("S1", "A1", "Bob"), cancellation.Token);

        _seatLocks.Verify(
            seatLocks => seatLocks.TryAcquireAsync("S1", "A1", "Bob", ConfiguredDuration, cancellation.Token),
            Times.Once);
    }

    [Theory]
    [InlineData(SeatLockOutcome.Acquired)]
    [InlineData(SeatLockOutcome.AlreadyHeldByRequester)]
    public async Task A_lock_the_customer_holds_is_accepted(SeatLockOutcome outcome)
    {
        ExistingSeat("S1", "A1");
        LockOutcomeIs(outcome);

        var result = await Handle("S1", "A1", "Bob");

        result.Should().Be(HoldSeatResult.Accepted());
    }

    [Fact]
    public async Task A_lock_held_by_another_customer_is_rejected()
    {
        ExistingSeat("S1", "A1");
        LockOutcomeIs(SeatLockOutcome.HeldByAnother);

        var result = await Handle("S1", "A1", "Jessica");

        result.Should().Be(HoldSeatResult.Rejected(HoldRejectionReason.SeatAlreadyHeld));
    }

    private void ExistingSeat(string sessionId, string seatId)
    {
        _schedule.Setup(schedule => schedule.SessionExists(sessionId)).Returns(true);
        _schedule.Setup(schedule => schedule.SeatExists(sessionId, seatId)).Returns(true);
    }

    private void LockOutcomeIs(SeatLockOutcome outcome) =>
        _seatLocks
            .Setup(seatLocks => seatLocks.TryAcquireAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(outcome);

    private void VerifyNoLockWasTaken() =>
        _seatLocks.Verify(
            seatLocks => seatLocks.TryAcquireAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    private ValueTask<HoldSeatResult> Handle(string sessionId, string seatId, string customerId) =>
        _handler.Handle(new HoldSeatCommand(sessionId, seatId, customerId), CancellationToken.None);
}
