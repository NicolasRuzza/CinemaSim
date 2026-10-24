using Cinema.BddTests.Drivers;

namespace Cinema.BddTests.StepDefinitions;

[Binding]
public sealed class HoldSeatStepDefinitions(SeatHoldDriver driver)
{
    [Given("a session {string} with the seats {string} and {string} available")]
    public void GivenASessionWithTheSeatsAvailable(string sessionId, string firstSeatId, string secondSeatId) =>
        driver.AddSession(sessionId, firstSeatId, secondSeatId);

    [Given("a session {string} with the seat {string} available")]
    public void GivenASessionWithTheSeatAvailable(string sessionId, string seatId) =>
        driver.AddSession(sessionId, seatId);

    [When("customer {string} holds seat {string} of session {string}")]
    public async Task WhenCustomerHoldsSeatOfSession(string customerId, string seatId, string sessionId) =>
        await driver.HoldSeatAsync(customerId, seatId, sessionId);

    [Then("the hold is accepted")]
    public void ThenTheHoldIsAccepted() =>
        driver.LastResult.IsAccepted.Should().BeTrue("an available seat can be held");
}
