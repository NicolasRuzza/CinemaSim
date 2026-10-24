using Mediator;

namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Asks the system to temporarily hold one seat of a session for a customer.</summary>
public sealed record HoldSeatCommand(string SessionId, string SeatId, string CustomerId)
    : ICommand<HoldSeatResult>;
