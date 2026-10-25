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

    [Fact]
    public async Task Holding_a_seat_held_by_another_customer_returns_409_as_problem_details()
    {
        await Hold("S1", "A1", "Bob");

        var response = await Hold("S1", "A1", "Jessica");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Holding_a_seat_of_a_nonexistent_session_returns_404_as_problem_details()
    {
        var response = await Hold("S9", "A1", "Bob");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Holding_a_nonexistent_seat_returns_404_as_problem_details()
    {
        var response = await Hold("S1", "Z9", "Bob");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    private Task<HttpResponseMessage> Hold(string sessionId, string seatId, string customerId) =>
        _factory.CreateClient().PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/seats/hold",
            new { seatId, customerId });
}
