namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Settings of the hold seat use case.</summary>
public sealed class HoldSeatOptions
{
    /// <summary>How long a hold keeps the seat reserved.</summary>
    public TimeSpan HoldDuration { get; set; } = TimeSpan.FromMinutes(5);
}
