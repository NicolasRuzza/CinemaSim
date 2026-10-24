using Cinema.Api.Features.Seats.HoldSeat;

namespace Cinema.BddTests.Support;

/// <summary>In-memory stand-in for the mocked cinema schedule, seeded by the feature Background.</summary>
public sealed class InMemorySessionSchedule : ISessionSchedule
{
    private readonly Dictionary<string, HashSet<string>> _seatsBySession = new();

    public void Add(string sessionId, IEnumerable<string> seatIds) =>
        _seatsBySession[sessionId] = [.. seatIds];

    public bool SessionExists(string sessionId) =>
        _seatsBySession.ContainsKey(sessionId);

    public bool SeatExists(string sessionId, string seatId) =>
        _seatsBySession.TryGetValue(sessionId, out var seats) && seats.Contains(seatId);
}
