namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Outcome of a hold attempt. Expected business outcomes are returned, not thrown.</summary>
public sealed record HoldSeatResult
{
    private HoldSeatResult(bool isAccepted, HoldRejectionReason? rejectionReason)
    {
        IsAccepted = isAccepted;
        RejectionReason = rejectionReason;
    }

    public bool IsAccepted { get; }

    /// <summary>Why the hold was refused. Null when the hold was accepted.</summary>
    public HoldRejectionReason? RejectionReason { get; }

    public static HoldSeatResult Accepted() => new(true, null);

    public static HoldSeatResult Rejected(HoldRejectionReason reason) => new(false, reason);
}
