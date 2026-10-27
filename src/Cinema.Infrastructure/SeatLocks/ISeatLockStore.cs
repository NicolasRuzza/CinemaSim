namespace Cinema.Infrastructure.SeatLocks;

/// <summary>Port to the seat lock, shared by every host that works with seat holds.</summary>
public interface ISeatLockStore
{
    /// <summary>
    /// Atomically tries to hold a seat for a customer. Checking and taking the lock happen
    /// in a single operation, so two simultaneous callers can never both acquire it.
    /// </summary>
    /// <remarks>
    /// The lock expires exactly after <paramref name="duration"/>. Repeating the call for the same
    /// customer while the lock is active returns <see cref="SeatLockOutcome.AlreadyHeldByRequester"/>
    /// with the same reservation id and does not extend the expiration. A hold made after the lock
    /// expired is a new reservation with a new id.
    /// </remarks>
    ValueTask<SeatLockResult> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken);
}
