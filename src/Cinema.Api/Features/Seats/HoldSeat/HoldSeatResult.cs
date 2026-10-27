namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Outcome of a hold attempt. Expected business outcomes are returned, not thrown.</summary>
public sealed record HoldSeatResult
{
    private HoldSeatResult(bool isAccepted, HoldRejectionReason? rejectionReason, Guid? reservationId)
    {
        IsAccepted = isAccepted;
        RejectionReason = rejectionReason;
        ReservationId = reservationId;
    }

    public bool IsAccepted { get; }

    /// <summary>Why the hold was refused. Null when the hold was accepted.</summary>
    public HoldRejectionReason? RejectionReason { get; }

    /// <summary>The reservation that holds the seat. Null when the hold was rejected.</summary>
    public Guid? ReservationId { get; }

    public static HoldSeatResult Accepted(Guid reservationId) => new(true, null, reservationId);

    public static HoldSeatResult Rejected(HoldRejectionReason reason) => new(false, reason, null);
}
