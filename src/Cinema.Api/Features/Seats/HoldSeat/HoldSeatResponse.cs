namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Echoes the hold that was accepted.</summary>
public sealed record HoldSeatResponse(string SessionId, string SeatId, string CustomerId);
