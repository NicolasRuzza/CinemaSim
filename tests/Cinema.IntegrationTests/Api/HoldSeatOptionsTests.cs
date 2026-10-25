using AwesomeAssertions;
using Cinema.Api.Features.Seats.HoldSeat;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cinema.IntegrationTests.Api;

/// <summary>How the hold duration is configured, and what happens when it is wrong.</summary>
public sealed class HoldSeatOptionsTests
{
    [Fact]
    public void The_hold_lasts_five_minutes_by_default()
    {
        using var factory = new CinemaApiFactory();

        HoldDurationOf(factory).Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void The_hold_duration_comes_from_configuration()
    {
        using var baseFactory = new CinemaApiFactory();
        using var factory = baseFactory.WithWebHostBuilder(builder =>
            builder.UseSetting("HoldSeat:HoldDuration", "00:00:30"));

        HoldDurationOf(factory).Should().Be(TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:01:00")]
    public void A_hold_duration_that_is_not_positive_stops_the_application_from_starting(string duration)
    {
        using var baseFactory = new CinemaApiFactory();
        using var factory = baseFactory.WithWebHostBuilder(builder =>
            builder.UseSetting("HoldSeat:HoldDuration", duration));

        var start = () => factory.Services;

        start.Should().Throw<OptionsValidationException>();
    }

    private static TimeSpan HoldDurationOf(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<IOptions<HoldSeatOptions>>().Value.HoldDuration;
}
