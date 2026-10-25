using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;

namespace Cinema.IntegrationTests.Api;

/// <summary>HTTP behavior of POST /api/v1/sessions/{sessionId}/seats/hold. Each test gets its own API.</summary>
public sealed class HoldSeatEndpointTests : IDisposable
{
    private readonly CinemaApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Holding_an_available_seat_returns_200()
    {
        var response = await Hold("S1", "A1", "Bob");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private Task<HttpResponseMessage> Hold(string sessionId, string seatId, string customerId) =>
        _factory.CreateClient().PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/seats/hold",
            new { seatId, customerId });
}
