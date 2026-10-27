using System.Net.Http.Json;
using AwesomeAssertions;

namespace Cinema.IntegrationTests.Api;

/// <summary>The part of the hold response these tests read. It is declared here so the tests do not depend on the API's own response type.</summary>
internal sealed record HoldResponseBody(Guid? ReservationId)
{
    /// <summary>Asserts the id is in the body before returning it, so two missing ids can never look equal.</summary>
    public static async Task<Guid> ReadReservationIdAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<HoldResponseBody>();

        body!.ReservationId.Should().HaveValue("the body of an accepted hold carries the reservation id");
        return body.ReservationId!.Value;
    }
}
