using Cinema.Api.Features.Seats.HoldSeat;

namespace Cinema.BddTests.Support;

/// <summary>In-memory stand-in for the Redis seat lock. It honors the same atomic contract.</summary>
public sealed class InMemorySeatLockStore : ISeatLockStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<(string SessionId, string SeatId), string> _holders = new();

    public ValueTask<SeatLockOutcome> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var key = (sessionId, seatId);

            if (_holders.TryGetValue(key, out var holder))
            {
                return ValueTask.FromResult(holder == customerId
                    ? SeatLockOutcome.AlreadyHeldByRequester
                    : SeatLockOutcome.HeldByAnother);
            }

            _holders[key] = customerId;
            return ValueTask.FromResult(SeatLockOutcome.Acquired);
        }
    }
}
