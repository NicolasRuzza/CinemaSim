namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Port to the seat lock, owned by the hold seat use case.</summary>
public interface ISeatLockStore
{
    /// <summary>
    /// Atomically tries to hold a seat for a customer. Checking and taking the lock happen
    /// in a single operation, so two simultaneous callers can never both acquire it.
    /// </summary>
    /// <remarks>
    /// The lock expires exactly after <paramref name="duration"/>. Repeating the call for the same
    /// customer while the lock is active returns <see cref="SeatLockOutcome.AlreadyHeldByRequester"/>
    /// and does not extend the expiration.
    /// </remarks>
    ValueTask<SeatLockOutcome> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken);
}
