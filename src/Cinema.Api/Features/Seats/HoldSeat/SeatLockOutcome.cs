namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Result of trying to take the lock of a seat.</summary>
public enum SeatLockOutcome
{
    /// <summary>The seat was free and is now held by the requester.</summary>
    Acquired,

    /// <summary>The requester already held the seat; nothing changed.</summary>
    AlreadyHeldByRequester,

    /// <summary>Another customer holds the seat.</summary>
    HeldByAnother,
}
