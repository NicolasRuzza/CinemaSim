using System.Net;
using System.Net.Http.Json;
using System.Text;
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

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"customerId":"Bob"}""")]
    [InlineData("""{"seatId":"A1"}""")]
    [InlineData("""{"seatId":"","customerId":"Bob"}""")]
    [InlineData("""{"seatId":"A1","customerId":"   "}""")]
    public async Task Holding_with_a_missing_or_blank_field_returns_400_as_problem_details(string json)
    {
        var response = await PostJson("S1", json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Repeating_a_hold_by_the_same_customer_returns_200()
    {
        await Hold("S1", "A1", "Bob");

        var response = await Hold("S1", "A1", "Bob");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Holding_an_available_seat_returns_its_reservation_id()
    {
        var response = await Hold("S1", "A1", "Bob");

        (await HoldResponseBody.ReadReservationIdAsync(response)).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Repeating_a_hold_by_the_same_customer_returns_the_same_reservation_id()
    {
        var first = await HoldResponseBody.ReadReservationIdAsync(await Hold("S1", "A1", "Bob"));

        var second = await HoldResponseBody.ReadReservationIdAsync(await Hold("S1", "A1", "Bob"));

        second.Should().Be(first);
    }

    private Task<HttpResponseMessage> PostJson(string sessionId, string json) =>
        _factory.CreateClient().PostAsync(
            $"/api/v1/sessions/{sessionId}/seats/hold",
            new StringContent(json, Encoding.UTF8, "application/json"));

    private Task<HttpResponseMessage> Hold(string sessionId, string seatId, string customerId) =>
        _factory.CreateClient().PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/seats/hold",
            new { seatId, customerId });
}
