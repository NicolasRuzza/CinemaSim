namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Port to the cinema schedule, owned by the hold seat use case.</summary>
public interface ISessionSchedule
{
    bool SessionExists(string sessionId);

    bool SeatExists(string sessionId, string seatId);
}
