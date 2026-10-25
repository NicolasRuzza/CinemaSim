using Mediator;

namespace Cinema.Api.Features.Seats.HoldSeat;

public sealed class HoldSeatHandler(ISessionSchedule schedule, ISeatLockStore seatLocks)
    : ICommandHandler<HoldSeatCommand, HoldSeatResult>
{
    public async ValueTask<HoldSeatResult> Handle(HoldSeatCommand command, CancellationToken cancellationToken)
    {
        if (!schedule.SessionExists(command.SessionId))
        {
            return HoldSeatResult.Rejected(HoldRejectionReason.SessionNotFound);
        }

        var outcome = await seatLocks.TryAcquireAsync(
            command.SessionId,
            command.SeatId,
            command.CustomerId,
            cancellationToken);

        return outcome == SeatLockOutcome.HeldByAnother
            ? HoldSeatResult.Rejected(HoldRejectionReason.SeatAlreadyHeld)
            : HoldSeatResult.Accepted();
    }
}
