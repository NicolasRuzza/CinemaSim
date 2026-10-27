using AwesomeAssertions;
using Cinema.Infrastructure.SeatLocks;

namespace Cinema.IntegrationTests.SeatLock;

/// <summary>
/// Rules every <see cref="ISeatLockStore"/> must honor. Each implementation runs the same tests,
/// so the in-memory fake used by the BDD scenarios cannot drift from the real Redis lock.
/// </summary>
public abstract class SeatLockStoreContract
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromSeconds(1);

    private ISeatLockStore? _store;

    // A unique session per test keeps tests independent when they share one Redis.
    private string SessionId { get; } = Guid.NewGuid().ToString("N");

    private ISeatLockStore Store => _store ??= CreateStore();

    protected abstract ISeatLockStore CreateStore();

    /// <summary>Lets time pass: instantly for a fake clock, for real against Redis.</summary>
    protected abstract Task WaitAsync(TimeSpan time);

    [Fact]
    public async Task A_free_seat_is_acquired()
    {
        (await Acquire("Bob")).Should().Be(SeatLockOutcome.Acquired);
    }

    [Fact]
    public async Task The_same_customer_repeating_gets_the_seat_it_already_holds()
    {
        await Acquire("Bob");

        (await Acquire("Bob")).Should().Be(SeatLockOutcome.AlreadyHeldByRequester);
    }

    [Fact]
    public async Task Another_customer_cannot_take_a_held_seat()
    {
        await Acquire("Bob");

        (await Acquire("Jessica")).Should().Be(SeatLockOutcome.HeldByAnother);
    }

    [Fact]
    public async Task Holding_a_seat_does_not_block_a_different_seat_of_the_same_session()
    {
        await Acquire("Bob", seatId: "A1");

        (await Acquire("Jessica", seatId: "A2")).Should().Be(SeatLockOutcome.Acquired);
    }

    [Fact]
    public async Task Holding_a_seat_does_not_block_the_same_seat_of_another_session()
    {
        await Acquire("Bob", sessionId: SessionId);

        (await Acquire("Jessica", sessionId: SessionId + "-other")).Should().Be(SeatLockOutcome.Acquired);
    }

    [Fact]
    public async Task A_hold_is_still_active_before_it_expires()
    {
        await Acquire("Bob");
        await WaitAsync(HoldDuration * 0.25);

        (await Acquire("Jessica")).Should().Be(SeatLockOutcome.HeldByAnother);
    }

    [Fact]
    public async Task A_hold_expires_after_its_duration()
    {
        await Acquire("Bob");
        await WaitAsync(HoldDuration * 1.5);

        (await Acquire("Jessica")).Should().Be(SeatLockOutcome.Acquired);
    }

    [Fact]
    public async Task A_repeated_hold_does_not_extend_the_duration()
    {
        await Acquire("Bob");
        await WaitAsync(HoldDuration * 0.35);
        (await Acquire("Bob")).Should().Be(SeatLockOutcome.AlreadyHeldByRequester);
        await WaitAsync(HoldDuration * 0.85);

        (await Acquire("Jessica")).Should().Be(SeatLockOutcome.Acquired);
    }

    [Fact]
    public async Task A_free_seat_is_acquired_with_a_reservation_id()
    {
        var result = await TryAcquire("Bob");

        result.ReservationId.Should().HaveValue("a held seat comes with its reservation");
        result.ReservationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task The_same_customer_repeating_gets_the_same_reservation_id()
    {
        var first = ReservationIdOf(await TryAcquire("Bob"));

        var second = ReservationIdOf(await TryAcquire("Bob"));

        second.Should().Be(first);
    }

    [Fact]
    public async Task A_new_hold_after_the_previous_one_expired_gets_a_different_reservation_id()
    {
        var first = ReservationIdOf(await TryAcquire("Bob"));
        await WaitAsync(HoldDuration * 1.5);

        var second = await TryAcquire("Bob");

        second.Outcome.Should().Be(SeatLockOutcome.Acquired);
        ReservationIdOf(second).Should().NotBe(first);
    }

    [Fact]
    public async Task Each_held_seat_has_its_own_reservation_id()
    {
        var first = ReservationIdOf(await TryAcquire("Bob", seatId: "A1"));

        var second = ReservationIdOf(await TryAcquire("Bob", seatId: "A2"));

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task Another_customer_is_never_given_the_reservation_id_of_the_holder()
    {
        await Acquire("Bob");

        var result = await TryAcquire("Jessica");

        result.Outcome.Should().Be(SeatLockOutcome.HeldByAnother);
        result.ReservationId.Should().BeNull("the reservation belongs to the holder alone");
    }

    [Fact]
    public async Task Only_one_of_many_simultaneous_acquisitions_wins()
    {
        const int customerCount = 20;
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var attempts = Enumerable.Range(1, customerCount)
            .Select(number => Task.Run(async () =>
            {
                await startSignal.Task;
                return await Acquire($"customer-{number}");
            }))
            .ToArray();

        startSignal.SetResult();
        var outcomes = await Task.WhenAll(attempts);

        outcomes.Count(outcome => outcome == SeatLockOutcome.Acquired).Should().Be(1);
        outcomes.Count(outcome => outcome == SeatLockOutcome.HeldByAnother).Should().Be(customerCount - 1);
    }

    private async ValueTask<SeatLockOutcome> Acquire(string customerId, string seatId = "A1", string? sessionId = null) =>
        (await TryAcquire(customerId, seatId, sessionId)).Outcome;

    private ValueTask<SeatLockResult> TryAcquire(string customerId, string seatId = "A1", string? sessionId = null) =>
        Store.TryAcquireAsync(sessionId ?? SessionId, seatId, customerId, HoldDuration, CancellationToken.None);

    // Asserts the id exists before it is compared, so two missing ids can never look equal.
    private static Guid ReservationIdOf(SeatLockResult result)
    {
        result.ReservationId.Should().HaveValue("a seat the customer holds comes with its reservation");
        return result.ReservationId!.Value;
    }
}
