namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Outcome of a hold attempt. Expected business outcomes are returned, not thrown.</summary>
public sealed record HoldSeatResult(bool IsAccepted)
{
    public static HoldSeatResult Accepted() => new(true);

    public static HoldSeatResult Rejected() => new(false);
}
