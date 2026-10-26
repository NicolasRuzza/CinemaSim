using Cinema.BddTests.Support;
using Cinema.Infrastructure.SeatLocks;
using Microsoft.Extensions.Time.Testing;

namespace Cinema.IntegrationTests.SeatLock;

/// <summary>Runs the seat lock contract against the in-memory fake used by the BDD scenarios.</summary>
public sealed class InMemorySeatLockStoreContractTests : SeatLockStoreContract
{
    private readonly FakeTimeProvider _clock = new();

    protected override ISeatLockStore CreateStore() => new InMemorySeatLockStore(_clock);

    protected override Task WaitAsync(TimeSpan time)
    {
        _clock.Advance(time);
        return Task.CompletedTask;
    }
}
