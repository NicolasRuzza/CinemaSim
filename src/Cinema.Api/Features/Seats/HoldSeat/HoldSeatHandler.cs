using Mediator;

namespace Cinema.Api.Features.Seats.HoldSeat;

public sealed class HoldSeatHandler : ICommandHandler<HoldSeatCommand, HoldSeatResult>
{
    // Simplest behavior that passes the first scenario; later scenarios drive the real rules.
    public ValueTask<HoldSeatResult> Handle(HoldSeatCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromResult(HoldSeatResult.Accepted());
}
