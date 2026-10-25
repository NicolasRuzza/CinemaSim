using Mediator;

namespace Cinema.Api.Features.Seats.HoldSeat;

public sealed class HoldSeatHandler(ISeatLockStore seatLocks) : ICommandHandler<HoldSeatCommand, HoldSeatResult>
{
    public async ValueTask<HoldSeatResult> Handle(HoldSeatCommand command, CancellationToken cancellationToken)
    {
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
