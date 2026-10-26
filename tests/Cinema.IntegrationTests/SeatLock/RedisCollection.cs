namespace Cinema.IntegrationTests.SeatLock;

/// <summary>
/// Puts every test that needs Redis in one collection: they run one after another, so the
/// time-based tests do not compete for CPU, and they share a single container.
/// </summary>
[CollectionDefinition("Redis")]
public sealed class RedisCollection : ICollectionFixture<RedisFixture>;
