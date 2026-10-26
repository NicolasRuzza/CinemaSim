using Cinema.Api.Features.Seats.HoldSeat;

namespace Cinema.IntegrationTests.SeatLock;

/// <summary>Runs the seat lock contract against a real Redis. Needs Docker.</summary>
[Trait("Requires", "Docker")]
[Collection("Redis")]
public sealed class RedisSeatLockStoreContractTests(RedisFixture redis) : SeatLockStoreContract
{
    protected override ISeatLockStore CreateStore() => new RedisSeatLockStore(redis.Connection);

    // Redis expires keys on its own clock, so against it time really has to pass.
    protected override Task WaitAsync(TimeSpan time) => Task.Delay(time);
}
