using StackExchange.Redis;

namespace Cinema.Infrastructure.SeatLocks;

/// <summary>
/// Seat lock backed by Redis. Redis runs the script below as a single atomic step, so checking
/// who holds the seat and taking it can never interleave with another caller.
/// </summary>
public sealed class RedisSeatLockStore(IConnectionMultiplexer redis) : ISeatLockStore
{
    // KEYS[1] = lock key, ARGV[1] = customer, ARGV[2] = duration in milliseconds.
    // Returns 1 when acquired, 2 when the requester already holds it (expiration untouched),
    // and 3 when another customer holds it.
    private const string AcquireScript = """
        local holder = redis.call('GET', KEYS[1])
        if not holder then
            redis.call('SET', KEYS[1], ARGV[1], 'PX', ARGV[2])
            return 1
        end
        if holder == ARGV[1] then
            return 2
        end
        return 3
        """;

    public async ValueTask<SeatLockResult> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        var result = (int)await redis.GetDatabase()
            .ScriptEvaluateAsync(
                AcquireScript,
                [KeyFor(sessionId, seatId)],
                [customerId, (long)duration.TotalMilliseconds])
            .WaitAsync(cancellationToken);

        var outcome = result switch
        {
            1 => SeatLockOutcome.Acquired,
            2 => SeatLockOutcome.AlreadyHeldByRequester,
            3 => SeatLockOutcome.HeldByAnother,
            _ => throw new InvalidOperationException($"Unexpected result {result} from the seat lock script."),
        };

        return new SeatLockResult(outcome, ReservationId: null);
    }

    private static RedisKey KeyFor(string sessionId, string seatId) => $"seat-hold:{sessionId}:{seatId}";
}
