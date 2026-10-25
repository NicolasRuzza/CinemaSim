namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Body of the hold seat endpoint. The session comes from the route.</summary>
public sealed record HoldSeatRequest(string SeatId, string CustomerId);
