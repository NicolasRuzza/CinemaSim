namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Echoes the hold that was accepted and names the reservation that holds the seat.</summary>
public sealed record HoldSeatResponse(string SessionId, string SeatId, string CustomerId, Guid ReservationId);
