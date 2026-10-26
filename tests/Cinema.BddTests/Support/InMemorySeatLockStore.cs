using Cinema.Infrastructure.SeatLocks;

namespace Cinema.BddTests.Support;

/// <summary>In-memory stand-in for the Redis seat lock. It honors the same atomic contract.</summary>
public sealed class InMemorySeatLockStore(TimeProvider clock) : ISeatLockStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<(string SessionId, string SeatId), (string CustomerId, DateTimeOffset ExpiresAt)> _holds = new();

    public ValueTask<SeatLockOutcome> TryAcquireAsync(
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
                    ? SeatLockOutcome.AlreadyHeldByRequester
                    : SeatLockOutcome.HeldByAnother);
            }

            _holds[key] = (customerId, now + duration);
            return ValueTask.FromResult(SeatLockOutcome.Acquired);
        }
    }
}
