namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Port to the seat lock, owned by the hold seat use case.</summary>
public interface ISeatLockStore
{
    /// <summary>
    /// Atomically tries to hold a seat for a customer. Checking and taking the lock happen
    /// in a single operation, so two simultaneous callers can never both acquire it.
    /// </summary>
    ValueTask<SeatLockOutcome> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        CancellationToken cancellationToken);
}
