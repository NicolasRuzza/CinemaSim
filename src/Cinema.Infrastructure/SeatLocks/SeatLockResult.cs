namespace Cinema.Infrastructure.SeatLocks;

/// <summary>What happened when trying to take the lock of a seat, and the reservation that holds it.</summary>
public sealed record SeatLockResult(SeatLockOutcome Outcome, Guid? ReservationId);
