using Cinema.Api.Features.Seats.HoldSeat;
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

    [Given("customer {string} holds seat {string} of session {string}")]
    [When("customer {string} holds seat {string} of session {string}")]
    public async Task CustomerHoldsSeatOfSession(string customerId, string seatId, string sessionId) =>
        await driver.HoldSeatAsync(customerId, seatId, sessionId);

    [Then("the hold is accepted")]
    public void ThenTheHoldIsAccepted() =>
        driver.LastResult.IsAccepted.Should().BeTrue("an available seat can be held");

    private static readonly Dictionary<string, HoldRejectionReason> RejectionReasons = new()
    {
        ["the seat is already held"] = HoldRejectionReason.SeatAlreadyHeld,
        ["the session does not exist"] = HoldRejectionReason.SessionNotFound,
        ["the seat does not exist"] = HoldRejectionReason.SeatNotFound,
    };

    [Then(@"^the hold is rejected because (.+)$")]
    public void ThenTheHoldIsRejectedBecause(string reasonText)
    {
        if (!RejectionReasons.TryGetValue(reasonText, out var expectedReason))
        {
            Assert.Fail($"The scenario uses an unknown rejection reason: '{reasonText}'.");
        }

        driver.LastResult.IsAccepted.Should().BeFalse("the hold must be rejected");
        driver.LastResult.RejectionReason.Should().Be(expectedReason);
    }

    [Given("{int} minutes and {int} seconds have passed")]
    public void GivenMinutesAndSecondsHavePassed(int minutes, int seconds) =>
        driver.AdvanceTime(TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds));

    [Given("{int} minutes have passed")]
    public void GivenMinutesHavePassed(int minutes) =>
        driver.AdvanceTime(TimeSpan.FromMinutes(minutes));

    [When("{int} different customers try to hold seat {string} of session {string} at the same time")]
    public async Task WhenCustomersTryToHoldSeatAtTheSameTime(int customerCount, string seatId, string sessionId) =>
        await driver.HoldSeatSimultaneouslyAsync(customerCount, seatId, sessionId);

    [Then("exactly {int} hold is accepted")]
    public void ThenExactlyHoldIsAccepted(int expected) =>
        driver.LastResults.Count(result => result.IsAccepted).Should().Be(expected);

    [Then("the other {int} holds are rejected because the seat is already held")]
    public void ThenTheOtherHoldsAreRejectedBecauseTheSeatIsAlreadyHeld(int expected)
    {
        var rejected = driver.LastResults.Where(result => !result.IsAccepted).ToList();

        rejected.Should().HaveCount(expected);
        rejected.Should().OnlyContain(result => result.RejectionReason == HoldRejectionReason.SeatAlreadyHeld);
    }
}
