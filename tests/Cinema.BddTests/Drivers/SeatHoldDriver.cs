using Cinema.Api.Features.Seats.HoldSeat;
using Cinema.BddTests.Support;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Cinema.BddTests.Drivers;

/// <summary>
/// Operates the system the way a kiosk would. Step definitions talk to this class and
/// never to the use case directly, so how the system is invoked can change in one place.
/// </summary>
public sealed class SeatHoldDriver
{
    private readonly InMemorySessionSchedule _schedule = new();
    private readonly FakeTimeProvider _clock = new();
    private readonly IMediator _mediator;
    private IReadOnlyList<HoldSeatResult> _lastResults = [];

    public SeatHoldDriver()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<ISessionSchedule>(_schedule);
        services.AddOptions<HoldSeatOptions>();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddSingleton<ISeatLockStore, InMemorySeatLockStore>();
        _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    /// <summary>The result of the last single hold attempt.</summary>
    public HoldSeatResult LastResult =>
        _lastResults.Count == 1
            ? _lastResults[0]
            : throw new InvalidOperationException($"Expected a single hold result, but there are {_lastResults.Count}.");

    /// <summary>The results of the last operation: one for a single hold, many for a simultaneous batch.</summary>
    public IReadOnlyList<HoldSeatResult> LastResults => _lastResults;

    public void AddSession(string sessionId, params string[] seatIds) =>
        _schedule.Add(sessionId, seatIds);

    /// <summary>Moves the scenario clock forward instantly, without waiting.</summary>
    public void AdvanceTime(TimeSpan elapsed) =>
        _clock.Advance(elapsed);

    public async Task HoldSeatAsync(string customerId, string seatId, string sessionId) =>
        _lastResults = [await _mediator.Send(new HoldSeatCommand(sessionId, seatId, customerId))];

    /// <summary>Fires one hold per customer at once; every attempt waits for the same start signal.</summary>
    public async Task HoldSeatSimultaneouslyAsync(int customerCount, string seatId, string sessionId)
    {
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var attempts = Enumerable.Range(1, customerCount)
            .Select(number => Task.Run(async () =>
            {
                await startSignal.Task;
                return await _mediator.Send(new HoldSeatCommand(sessionId, seatId, $"customer-{number}"));
            }))
            .ToArray();

        startSignal.SetResult();
        _lastResults = await Task.WhenAll(attempts);
    }
}
