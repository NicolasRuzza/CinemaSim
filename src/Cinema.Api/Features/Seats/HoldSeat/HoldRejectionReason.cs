namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Why a hold attempt was refused.</summary>
public enum HoldRejectionReason
{
    SeatAlreadyHeld,
    SessionNotFound,
}
