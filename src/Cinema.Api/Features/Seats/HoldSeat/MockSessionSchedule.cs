namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>
/// Phase 1 schedule: five sessions (S1 to S5), each with seats A1 to E8.
/// It will be replaced by the real catalog when movies come from the database.
/// </summary>
public sealed class MockSessionSchedule : ISessionSchedule
{
    private static readonly HashSet<string> SessionIds = ["S1", "S2", "S3", "S4", "S5"];

    private static readonly HashSet<string> SeatIds =
    [
        .. from row in "ABCDE"
           from number in Enumerable.Range(1, 8)
           select $"{row}{number}",
    ];

    public bool SessionExists(string sessionId) => SessionIds.Contains(sessionId);

    public bool SeatExists(string sessionId, string seatId) =>
        SessionExists(sessionId) && SeatIds.Contains(seatId);
}
