using StackExchange.Redis;

namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Seat lock backed by Redis.</summary>
public sealed class RedisSeatLockStore(IConnectionMultiplexer redis) : ISeatLockStore
{
    // Red phase: accepts everything on purpose so the contract tests fail on their assertions.
    public ValueTask<SeatLockOutcome> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken) =>
        ValueTask.FromResult(SeatLockOutcome.Acquired);
}
