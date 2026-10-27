using Cinema.Infrastructure.SeatLocks;

namespace Cinema.BddTests.Support;

/// <summary>In-memory stand-in for the Redis seat lock. It honors the same atomic contract.</summary>
public sealed class InMemorySeatLockStore(TimeProvider clock) : ISeatLockStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<(string SessionId, string SeatId), (string CustomerId, Guid ReservationId, DateTimeOffset ExpiresAt)> _holds = new();

    public ValueTask<SeatLockResult> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var key = (sessionId, seatId);
            var now = clock.GetUtcNow();

            // A hold is active until the instant it expires; at that exact instant it is already gone.
            if (_holds.TryGetValue(key, out var hold) && hold.ExpiresAt > now)
            {
                return ValueTask.FromResult(hold.CustomerId == customerId
                    ? new SeatLockResult(SeatLockOutcome.AlreadyHeldByRequester, hold.ReservationId)
                    : new SeatLockResult(SeatLockOutcome.HeldByAnother, ReservationId: null));
            }

            var reservationId = Guid.NewGuid();
            _holds[key] = (customerId, reservationId, now + duration);
            return ValueTask.FromResult(new SeatLockResult(SeatLockOutcome.Acquired, reservationId));
        }
    }
}
