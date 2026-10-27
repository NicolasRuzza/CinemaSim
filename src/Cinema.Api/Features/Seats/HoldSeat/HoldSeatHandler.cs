using Cinema.Infrastructure.SeatLocks;
using Mediator;
using Microsoft.Extensions.Options;

namespace Cinema.Api.Features.Seats.HoldSeat;

public sealed class HoldSeatHandler(
    ISessionSchedule schedule,
    ISeatLockStore seatLocks,
    IOptions<HoldSeatOptions> options)
    : ICommandHandler<HoldSeatCommand, HoldSeatResult>
{
    public async ValueTask<HoldSeatResult> Handle(HoldSeatCommand command, CancellationToken cancellationToken)
    {
        if (!schedule.SessionExists(command.SessionId))
        {
            return HoldSeatResult.Rejected(HoldRejectionReason.SessionNotFound);
        }

        if (!schedule.SeatExists(command.SessionId, command.SeatId))
        {
            return HoldSeatResult.Rejected(HoldRejectionReason.SeatNotFound);
        }

        var lockResult = await seatLocks.TryAcquireAsync(
            command.SessionId,
            command.SeatId,
            command.CustomerId,
            options.Value.HoldDuration,
            cancellationToken);

        // The seat lock contract guarantees a reservation id whenever the requester holds the seat.
        return lockResult.Outcome == SeatLockOutcome.HeldByAnother
            ? HoldSeatResult.Rejected(HoldRejectionReason.SeatAlreadyHeld)
            : HoldSeatResult.Accepted(lockResult.ReservationId!.Value);
    }
}
