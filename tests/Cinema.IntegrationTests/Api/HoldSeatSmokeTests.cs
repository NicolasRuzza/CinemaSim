using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Cinema.IntegrationTests.SeatLock;

namespace Cinema.IntegrationTests.Api;

/// <summary>The whole stack against a real Redis: HTTP, Mediator, handler and Lua script. Needs Docker.</summary>
[Trait("Requires", "Docker")]
[Collection("Redis")]
public sealed class HoldSeatSmokeTests(RedisFixture redis) : IDisposable
{
    private readonly RedisApiFactory _factory = new(redis.ConnectionString);

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Holding_a_seat_goes_through_the_whole_stack_and_leaves_a_five_minute_lock_in_redis()
    {
        var client = _factory.CreateClient();

        var bob = await Hold(client, "S2", "C3", "Bob");
        var jessica = await Hold(client, "S2", "C3", "Jessica");
        var bobAgain = await Hold(client, "S2", "C3", "Bob");

        bob.StatusCode.Should().Be(HttpStatusCode.OK);
        jessica.StatusCode.Should().Be(HttpStatusCode.Conflict);
        bobAgain.StatusCode.Should().Be(HttpStatusCode.OK);

        var timeToLive = await redis.Connection.GetDatabase().KeyTimeToLiveAsync("seat-hold:S2:C3");

        timeToLive.Should().NotBeNull();
        timeToLive!.Value.Should().BeGreaterThan(TimeSpan.FromMinutes(4));
        timeToLive.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Twenty_simultaneous_requests_for_one_seat_yield_exactly_one_200()
    {
        var client = _factory.CreateClient();
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var requests = Enumerable.Range(1, 20)
            .Select(number => Task.Run(async () =>
            {
                await startSignal.Task;
                return await Hold(client, "S3", "D4", $"customer-{number}");
            }))
            .ToArray();

        startSignal.SetResult();
        var responses = await Task.WhenAll(requests);

        responses.Count(response => response.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(19);
    }

    private static Task<HttpResponseMessage> Hold(HttpClient client, string sessionId, string seatId, string customerId) =>
        client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/seats/hold", new { seatId, customerId });
}
