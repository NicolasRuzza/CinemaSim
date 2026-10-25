using Cinema.Api.Features.Seats.HoldSeat;
using Cinema.BddTests.Support;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.BddTests.Drivers;

/// <summary>
/// Operates the system the way a kiosk would. Step definitions talk to this class and
/// never to the use case directly, so how the system is invoked can change in one place.
/// </summary>
public sealed class SeatHoldDriver
{
    private readonly InMemorySessionSchedule _schedule = new();
    private readonly IMediator _mediator;
    private HoldSeatResult? _lastResult;

    public SeatHoldDriver()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<ISessionSchedule>(_schedule);
        services.AddSingleton<ISeatLockStore, InMemorySeatLockStore>();
        _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    public HoldSeatResult LastResult =>
        _lastResult ?? throw new InvalidOperationException("No hold was attempted in this scenario.");

    public void AddSession(string sessionId, params string[] seatIds) =>
        _schedule.Add(sessionId, seatIds);

    public async Task HoldSeatAsync(string customerId, string seatId, string sessionId) =>
        _lastResult = await _mediator.Send(new HoldSeatCommand(sessionId, seatId, customerId));
}
