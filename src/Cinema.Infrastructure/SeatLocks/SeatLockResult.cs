namespace Cinema.Infrastructure.SeatLocks;

/// <summary>What happened when trying to take the lock of a seat, and the reservation that holds it.</summary>
/// <param name="Outcome">Whether the seat was taken, was already held by the requester, or is held by another customer.</param>
/// <param name="ReservationId">
/// The reservation that holds the seat: a new one when acquired, the existing one when the requester repeats the call.
/// Null when another customer holds the seat, because that reservation is not the requester's to know.
/// </param>
public sealed record SeatLockResult(SeatLockOutcome Outcome, Guid? ReservationId);
