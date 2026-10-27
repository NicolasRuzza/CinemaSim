using StackExchange.Redis;

namespace Cinema.Infrastructure.SeatLocks;

/// <summary>
/// Seat lock backed by Redis. Redis runs the script below as a single atomic step, so checking
/// who holds the seat and taking it can never interleave with another caller.
/// </summary>
public sealed class RedisSeatLockStore(IConnectionMultiplexer redis) : ISeatLockStore
{
    // The lock is a hash with the fields customer and reservation, and the hash expires as a whole.
    // KEYS[1] = lock key, ARGV[1] = customer, ARGV[2] = duration in milliseconds,
    // ARGV[3] = the reservation id to use if the seat is free.
    // Returns {1, id} when acquired, {2, id} when the requester already holds it (expiration untouched),
    // and {3} when another customer holds it: the holder's reservation is never revealed.
    private const string AcquireScript = """
        local hold = redis.call('HMGET', KEYS[1], 'customer', 'reservation')
        if not hold[1] then
            redis.call('HSET', KEYS[1], 'customer', ARGV[1], 'reservation', ARGV[3])
            redis.call('PEXPIRE', KEYS[1], ARGV[2])
            return {1, ARGV[3]}
        end
        if hold[1] == ARGV[1] then
            return {2, hold[2]}
        end
        return {3}
        """;

    public async ValueTask<SeatLockResult> TryAcquireAsync(
        string sessionId,
        string seatId,
        string customerId,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        var reply = (RedisResult[]?)await redis.GetDatabase()
            .ScriptEvaluateAsync(
                AcquireScript,
                [KeyFor(sessionId, seatId)],
                [customerId, (long)duration.TotalMilliseconds, Guid.NewGuid().ToString()])
            .WaitAsync(cancellationToken)
            ?? throw new InvalidOperationException("The seat lock script returned no result.");

        var code = (int)reply[0];
        var outcome = code switch
        {
            1 => SeatLockOutcome.Acquired,
            2 => SeatLockOutcome.AlreadyHeldByRequester,
            3 => SeatLockOutcome.HeldByAnother,
            _ => throw new InvalidOperationException($"Unexpected result {code} from the seat lock script."),
        };

        Guid? reservationId = outcome == SeatLockOutcome.HeldByAnother ? null : Guid.Parse(reply[1].ToString());
        return new SeatLockResult(outcome, reservationId);
    }

    private static RedisKey KeyFor(string sessionId, string seatId) => $"seat-hold:{sessionId}:{seatId}";
}
